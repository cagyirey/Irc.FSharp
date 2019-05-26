namespace Irc.FSharp

open System
open System.Net
open System.Text

open Irc.FSharp.IRCv3

type IrcServerState = { 
    Name: string
    Version: string
    MessageOfTheDay: string option
    UserModes: char list
    ChannelModes: char list
    ServerModes: char list
    Supports: Map<string, string option>
    Capabilities: Map<string, string option>
}

type IrcClientState = {
    Nickname: string
    Username: string
    HostMask: string
    
    UniqueId: string option
    Capabilities: Map<string, string option>
}

// Tested with InspIRCd  - other daemons may not work, especially if they don't use RPL_HOSTHIDDEN
/// Provides a high-level representation of an IRC session.
type IrcConnection(host: EndPoint, nickname, ?username, ?useSsl, ?validateCertCallback) as this =

    [<Literal>]
    let IRCv3Version = "302"

    let motdBuilder = StringBuilder()
    let readyEvt = Event<IrcClientState * IrcServerState> ()

    let serverState = ref {
        Name = String.Empty
        Version = String.Empty
        MessageOfTheDay = None
        UserModes = []
        ChannelModes = []
        ServerModes = []
        Supports = Map.empty
        Capabilities = Map.empty
    }

    let clientState = ref {
        Nickname = nickname
        Username = defaultArg username nickname
        HostMask = String.Empty
        UniqueId = None
        Capabilities = Map.empty
    }

    let rec addCapability (features: string list) 
        (source: Map<string, string option>) =
            match features with
            | [_] -> source // drop the last argument (a text 
            | feature :: xs -> 
                let k, v =
                    match feature.IndexOf '=' with
                    | -1 -> feature, None
                    | i -> feature.[0..i - 1], Some feature.[i + 1..]
                addCapability xs (source.Add (k, v))
            | [] -> source

    let addSupportedFeatures features = 
        addCapability features (!serverState).Supports

    let addClientCapabilities capabilities =
        addCapability capabilities (!clientState).Capabilities

    let addServerCapabilities capabilities =
        addCapability capabilities (!serverState).Capabilities

    let mutable client = 
        new IrcClient(host,
            defaultArg useSsl false,
            defaultArg validateCertCallback noSslErrors)

    let ircv3Handler = function
    | IRCv3.Capability "LIST" capabilities ->
        clientState :=
            { !clientState with
                Capabilities = addClientCapabilities capabilities }
    | IRCv3.Capability "LS" capabilities ->
        serverState :=
            { !serverState with
                Capabilities = addServerCapabilities capabilities }
        Capability.End |> client.WriteMessage

    let handleClientStateChange state = function
    | NumericResponse (int ResponseCode.RPL_YOURID) [_; id; _] -> 
        state := 
            { !state with
                UniqueId = Some id }
    | NumericResponse (int ResponseCode.RPL_HOSTHIDDEN) [_; mask; _]  ->
        state := 
            { !state with
                HostMask = mask }
        readyEvt.Trigger (!state, !serverState)
    | _ -> ()

    let handleServerStateChange state = function
    | NumericResponse (int ResponseCode.RPL_ISUPPORT) (_ :: features) -> 
        state :=
            { !state with
                Supports = addSupportedFeatures features }
    | NumericResponse (int ResponseCode.RPL_MYINFO) [_; serverName; version; uModes; chModes; serverModes] ->
        state :=
            { !state with
                Name = serverName
                Version = version
                UserModes = Seq.toList uModes
                ChannelModes = Seq.toList chModes
                ServerModes = Seq.toList serverModes }
    | NumericResponse (int ResponseCode.RPL_MOTD) [_; line] -> 
        motdBuilder.AppendLine line |> ignore
    | NumericResponse(int ResponseCode.RPL_ENDOFMOTD) _ ->
        let motd = motdBuilder.ToString()
        do motdBuilder.Clear() |> ignore
        state :=
            { !state with
                MessageOfTheDay = Some motd }
    | _ -> ()

    // These observers loop through communications looking for configuration values
    let handleServerStateChanged = 
        Observable.subscribe(handleServerStateChange serverState)
            client.MessageReceived
    
    let handleClientStateChanged =
        Observable.subscribe(handleClientStateChange clientState)
            client.MessageReceived

    // TODO: Attach handlers for IRCv3 and other persistent features here. `Event.filter` / `Event.choose` for performance.
    do
        client.MessageReceived
        |> Event.filter(fun msg -> equalsCI "CAP" msg.Command)
        |> Event.add ircv3Handler

        this.ConnectAsync() |> Async.Start

    new(host: string, port: int, nickname: string, ?username: string, ?useSsl: bool, ?validateCertCallback: Security.RemoteCertificateValidationCallback) = 
        IrcConnection(DnsEndPoint(host, port),
            nickname,
            defaultArg username nickname,
            defaultArg useSsl false,
            defaultArg validateCertCallback noSslErrors)

    member this.Client = client

    member this.ClientState = !clientState

    member this.ServerState = !serverState
    // TODO: use a better ready indicator (currently: RPL_HOSTHIDDEN)
    [<CLIEvent>]
    member this.OnReady = readyEvt.Publish

    member this.Reconnect () =
        if not client.Connected then
            client <- new IrcClient(host, defaultArg useSsl false, defaultArg validateCertCallback noSslErrors)
        else invalidOp "Reconnect was called on an open socket."

    member internal this.ConnectAsync () =
        async { 
            client.WriteMessage (Capability.Ls IRCv3Version)

            client.WriteMessage (IrcMessage.nick (!clientState).Nickname)
            client.WriteMessage (IrcMessage.user (!clientState).Username "0" (!clientState).Username)

            do! 
                Async.AwaitEvent (this.OnReady)
                |> Async.Ignore

            // After the connection is ready, we need to disconnect unneeded handlers
            // We will also want to connect a new set of events to watch events such as JOIN, NAMES, PART, etc

            //handleServerStateChanged.Dispose()
            //handleClientStateChanged.Dispose()
        }