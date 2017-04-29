# Irc.FSharp [![Appveyor build status](https://ci.appveyor.com/api/projects/status/phblrb0ix2g1kowr?svg=true)](https://ci.appveyor.com/project/cagyirey/irc-fsharp) [![Travis build status](https://travis-ci.org/cagyirey/Irc.FSharp.svg?branch=master)](https://travis-ci.org/cagyirey/Irc.FSharp)

Irc.FSharp is an IRC client library for F#. It provides a simple framework for connecting to IRC servers, sending and receiving standard IRC messages defined in RFC 2812.

### Installation

Build Irc.FSharp from the provided .sln file, `build.cmd` or `build.sh`. The solution is configured for .NET 4.6 and F# 4.0 by default.

### Getting Started

Irc.FSharp supports two approaches to message processing: asynchronous and event-based. To work with single inbound messages in a non-blocking way, use `IrcClient.NextMessage()`; the `IrcClient.MessageReceived` event provides a reactive API:

```fsharp
#r "Irc.FSharp.dll"

open Irc.FSharp
open Irc.FSharp.Net
open System.Windows.Forms

module Program = 
    [<EntryPoint>]
    let main argv = 
        // Configuration placeholder - FSharp.Configuration is a good option
        let host, port = Settings.Irc.Host, Settings.Irc.Port
        let nick, user = Settings.Irc.Nickname, Settings.Irc.Username
        let client = new Irclient(host, port, Settings.Irc.UseSSL, fun _ _ _ _ -> true)
        let channels = Settings.Irc.Channels

        client.MessageReceived
        |> Event.add (fun m -> printfn "%O" m)

        let handleMotd () =
            let rec loop () =
                async {
                    let! msg = client.NextMessage ()
                    match msg with
                    // Assume it's safe to join channels after RPL_ENDOFMOTD or ERR_NOMOTD.
                    | NumericResponse (int ResponseCode.RPL_ENDOFMOTD)
                    | NumericResponse (int ResponseCode.ERR_NOMOTD) ->
                        client.WriteMessage (IrcMessage.join channels)
                    | _ -> return! loop ()
                }
            loop ()

        do client.WriteMessage (IrcMessage.nick nick)
           client.WriteMessage (IrcMessage.user user "0" user)
        handleMotd () |> Async.RunSynchronously

        client.MessageReceived
        |> Event.choose(fun msg ->
            match msg with
            | PRIVMSG(User sender, target, message) when target = nick -> Some <| IrcMessage.privmsg [ sender ] "Hello!"
            | PRIVMSG(User sender, ch, message) -> Some <| IrcMessage.privmsg [ch] (sprintf "%s: Hello!" sender)
            | _ -> None)
        |> Event.add(client.WriteMessage)

	Application.Run()
	0
```

### Project Status

Irc.FSharp is currently in its alpha stage. It should be expected that there will be bugs, the final shape of the API may change, and some advanced features are incomplete.

### License

Irc.FSharp is available under the MIT license. For more information, see the [license file](https://github.com/cagyirey/Irc.FSharp/blob/master/LICENSE.md).
