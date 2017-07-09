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
        // "Parametric channel modes"
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

    let rec loop (features: string list) (acc: Map<string, string option>) =
            match features with
            | feature :: xs -> 
                let k, v =
                    match feature.IndexOf '=' with
                    | -1 -> feature, None
                    | i -> feature.[0..i - 1], Some feature.[i + 1..]
                loop xs (acc.Add (k, v))
            | [] -> acc

    let addSupportedFeatures features = 
        loop features (!serverState).Supports

    let addClientCapabilities capabilities =
        loop capabilities (!clientState).Capabilities

    let addServerCapabilities capabilities =
        loop capabilities (!serverState).Capabilities

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

    let clientNegotiate state = function
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

    let serverNegotiate state = function
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
    | NumericResponse (int ResponseCode.RPL_MOTD) [_; line] -> motdBuilder.AppendLine line |> ignore
    | NumericResponse(int ResponseCode.RPL_ENDOFMOTD) _ ->
        state :=
            { !state with
                MessageOfTheDay = Some (motdBuilder.ToString()) }
    | _ -> ()

    // These observers loop through communications looking for configuration values
    let serverNegotiator = 
        Observable.subscribe(serverNegotiate serverState) client.MessageReceived
    
    let clientNegotiator =
        Observable.subscribe(clientNegotiate clientState) client.MessageReceived

    // TODO: Attach handlers for IRCv3 and other persistent features here. `Event.filter` / `Event.choose` for performance.
    do
        client.MessageReceived
        |> Event.filter(fun msg -> equalsCI "CAP" msg.Command)
        |> Event.add ircv3Handler

    do this.ConnectAsync() |> Async.Start

    member this.Client = client

    member this.ClientState = !clientState

    member this.ServerState = !serverState
    // TODO: use a better ready indicator (currently: RPL_HOSTHIDDEN)
    [<CLIEvent>]
    member this.Ready = 
        readyEvt.Publish

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
                Async.AwaitEvent (this.Ready)
                |> Async.Ignore

            // After the connection is ready, we need to disconnect the event handlers for perf reasons
            // We will also want to connect a new set of events to watch events such as JOIN, NAMES, PART, etc
            serverNegotiator.Dispose(); clientNegotiator.Dispose();
        }