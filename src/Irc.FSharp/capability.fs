namespace Irc.FSharp

module IRCv3 =

    let (|Capability|_|) subcommand (msg: IrcMessage) =
        match msg with
        | IrcMessage(_, EqualsCI "CAP", _ :: cmd :: rest) ->
            if equalsCI subcommand cmd then
                Some rest
            else None
        | _ -> None

    module Capability =

        let internal capability subcommand args = IrcMessage(Empty, "CAP", subcommand :: args)

        let Ls version = capability "LS" [version]

        let List = capability "LIST" []

        let Req features = capability "REQ" (Seq.toList features)

        let Ack request = capability "ACK" (Seq.toList request)

        let Nak request = capability "NAK" [request]

        let End = capability "END" []