# Irc.FSharp [![Appveyor build status](https://ci.appveyor.com/api/projects/status/phblrb0ix2g1kowr?svg=true)](https://ci.appveyor.com/project/cagyirey/irc-fsharp) [![Travis build status](https://travis-ci.org/cagyirey/Irc.FSharp.svg?branch=master)](https://travis-ci.org/cagyirey/Irc.FSharp)

Irc.FSharp is an IRC client library for F#. It provides a simple framework for connecting to IRC servers, sending and receiving standard IRC messages defined in RFC 2812.

### Installation

Build Irc.FSharp from the provided .sln file, `build.cmd` or `build.sh`. The solution is configured for .NET 4.5 and F# 4.1 by default.

### Getting Started

Irc.FSharp supports two approaches to message processing: asynchronous and event-based. To work with single inbound messages in a non-blocking way, use `IrcClient.NextMessage()`; the `IrcClient.MessageReceived` event provides a reactive API. `IrcConnection` encapsulates an `IrcClient`, the connection state, and some common features of an IRC client:

```fsharp
#r "Irc.FSharp.dll"

open Irc.FSharp
open System.Windows.Forms

module Program = 
    [<EntryPoint>]
    let main argv =
        // Settings loaded by FSharp.Configuration - for example, a YAML or XML config file
        let host = DnsEndPoint(Settings.Irc.Host, Settings.Irc.Port)
        let nick, user = Settings.Irc.Nickname, Settings.Irc.Username
        let channels = Settings.Irc.Channels

        let con = new IrcConnection(host, nick, user, true, Net.Security.RemoteCertificateValidationCallback(fun _ _ _ _ -> true))
        let client = con.Client

        // Prints the client and server configuration information (as reported by the server)
        async {
            let! result = con.Ready |> Async.AwaitEvent
            printfn "%A\r\n\r\n%A" (fst result) (snd result)
        } |> Async.RunSynchronously

        // Transform an incoming message into a response with `Event.choose`, then send it
        // Greets the user upon receiving a direct message or a mention in a channel
        client.MessageReceived
        |> Event.choose(function
            | PRIVMSG(Nickname sender, target, message) when target = nick -> Some <| IrcMessage.privmsg [ sender ] "Hello!"
            | PRIVMSG(Nickname sender, ch, message) -> Some <| IrcMessage.privmsg [ch] (sprintf "%s: Hello!" sender)
            | _ -> None)
        |> Event.add(client.WriteMessage)

        Application.Run()
        0
```

### Project Status

Irc.FSharp is currently in its alpha stage. It should be expected that there will be bugs, the final shape of the API may change, and some advanced features are incomplete.

### License

Irc.FSharp is available under the MIT license. For more information, see the [license file](https://github.com/cagyirey/Irc.FSharp/blob/master/LICENSE.md).
