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

        let internal Cap subcommand args = IrcMessage(Empty, "CAP", subcommand :: args)

        let Ls version = Cap "LS" [version]

        let List = Cap "LIST" []

        let Req features = Cap "REQ" (Seq.toList features)

        let Ack request = Cap "ACK" (Seq.toList request)

        let Nak request = Cap "NAK" [request]

        let End = Cap "END" []