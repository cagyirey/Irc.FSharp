namespace Irc.FSharp

[<AutoOpen>]
module RFC2812 =

    type IrcMessage with

        static member pass password = IrcMessage(Empty, "PASS", [password])

        static member nick nickname = IrcMessage(Empty, "NICK", [nickname])

        static member user username mode realname = IrcMessage(Empty, "USER", [username; mode; "*"; realname])

        static member oper name password = IrcMessage(Empty, "OPER", [name; password])

        static member mode user modes = IrcMessage(Empty, "MODE", [user; modes])

        static member service nickname distribution ``type`` info = IrcMessage(Empty, "SERVICE", [nickname; "*"; distribution; ``type``; "*"; info])

        static member quit ?message = IrcMessage(Empty, "QUIT", Option.toList message)

        static member squit server comment = IrcMessage(Empty, "SQUIT", [server; comment])

        static member join channels = IrcMessage(Empty, "JOIN", [String.concat "," channels])

        static member join(channels, keys) =  IrcMessage(Empty, "JOIN", [String.concat "," channels; String.concat "," keys])

        static member part channels = IrcMessage(Empty, "PART", [String.concat "," channels])

        static member part(channels, message) = IrcMessage(Empty, "PART", [String.concat "," channels; message])

        static member topic channel = IrcMessage(Empty, "TOPIC", [channel])

        static member topic(channel, topic) = IrcMessage(Empty, "TOPIC", [channel; topic])

        static member names channels = IrcMessage(Empty, "NAMES", [String.concat "," channels])

        static member names(channels, target) = IrcMessage(Empty, "NAMES", String.concat "," channels :: Option.toList target)

        static member list channels = IrcMessage(Empty, "LIST",  [String.concat "," channels])

        static member list(channels, target) = IrcMessage(Empty, "LIST",  [String.concat "," channels; target])

        static member invite nickname channel = IrcMessage(Empty, "INVITE", [nickname; channel])

        static member kick channels users = IrcMessage(Empty, "KICK", [String.concat "," channels; String.concat "," users])

        static member privmsg recipients message = IrcMessage(Empty, "PRIVMSG", [String.concat "," recipients; message])

        static member notice msgtarget text = IrcMessage(Empty, "NOTICE", [msgtarget; text])

        static member motd ?target = IrcMessage(Empty, "MOTD", Option.toList target)

        static member lusers () = IrcMessage(Empty, "LUSERS", [])

        static member lusers mask = IrcMessage(Empty, "LUSERS", [mask])

        static member lusers (mask, target) = IrcMessage(Empty, "LUSERS", [mask; target])

        static member version ?target = IrcMessage(Empty, "VERSION", Option.toList target)

        static member stats () = IrcMessage(Empty, "STATS", [])

        static member stats query = IrcMessage(Empty, "STATS", [query])

        static member stats (query, target) = IrcMessage(Empty, "STATS", [query; target])

        static member links () = IrcMessage(Empty, "LINKS", [])

        static member links remoteServer = IrcMessage(Empty, "LINKS", [remoteServer])

        static member links (remoteServer, serverMask) = IrcMessage(Empty, "LINKS", [remoteServer; serverMask])

        static member time ?target = IrcMessage(Empty, "TIME", Option.toList target)

        static member connect (targetServer, port) = IrcMessage(Empty, "CONNECT", [targetServer; port])

        static member connect (targetServer, port, remoteServer) = IrcMessage(Empty, "CONNECT", [targetServer; port; remoteServer])

        static member trace ?target = IrcMessage(Empty, "TRACE", Option.toList target)

        static member admin ?target = IrcMessage(Empty, "ADMIN", Option.toList target)

        static member info ?target = IrcMessage(Empty, "INFO", Option.toList target)

        static member servlist () = IrcMessage(Empty, "SERVLIST", [])

        static member servlist mask = IrcMessage(Empty, "SERVLIST", [mask])

        static member servlist (mask, ``type``) = IrcMessage(Empty, "SERVLIST", [mask; ``type``])

        static member squery servicename text = IrcMessage(Empty, "SQUERY", [servicename; text])

        static member who mask = IrcMessage(Empty, "WHO", [mask])

        static member who (mask, operatorsOnly) = IrcMessage(Empty, "WHO", mask :: if operatorsOnly then ["o"] else [])

        static member whois mask = IrcMessage(Empty, "WHOIS", [mask])

        static member whois masks = IrcMessage(Empty, "WHOIS", [String.concat "," masks])

        static member whois (target, masks) = IrcMessage(Empty, "WHOIS", [target; String.concat "," masks])

        static member whowas nicknames = IrcMessage(Empty, "WHOWAS", [String.concat "," nicknames])

        static member whowas (nicknames, count) = IrcMessage(Empty, "WHOWAS", [String.concat "," nicknames; count])

        static member whowas (nicknames, count, target) = IrcMessage(Empty, "WHOWAS", [String.concat "," nicknames; count; target])

        static member kill nickname comment = IrcMessage(Empty, "KILL", [nickname; comment])

        static member ping server1 = IrcMessage(Empty, "PING", [server1])

        static member ping(server1, server2) = IrcMessage(Empty, "PING", [server1; server2])

        static member pong server1 = IrcMessage(Empty, "PONG", [server1])

        static member pong(server1, server2) = IrcMessage(Empty, "PONG", [server1; server2])

    let inline private (%=) (str1: string) (str2: string) = str1.Equals(str2, System.StringComparison.CurrentCultureIgnoreCase)

    let (|PASS|_|) (message: IrcMessage) =
        if message.Command %= "pass" then
            match message with
                | IrcMessage(sender, _, [password]) -> Some(sender, password)
                | _ -> None
        else None

    let (|NICK|_|) (message: IrcMessage) =
        if message.Command %= "nick" then
            match message with
                | IrcMessage(sender, _, [nickname]) -> Some(sender, nickname)
                | _ -> None
        else None

    let (|USER|_|) (message: IrcMessage) =
        if message.Command %= "user" then
            match message with
                | IrcMessage(sender, _, [username; mode; realname]) -> Some(sender, username, mode, realname)
                | _ -> None
        else None

    let (|OPER|_|) (message: IrcMessage) =
        if message.Command %= "oper" then
            match message with
                | IrcMessage(sender, _, [name; password]) -> Some(sender, name, password)
                | _ -> None
        else None

    let (|MODE|_|) (message: IrcMessage) =
        if message.Command %= "mode" then
            match message with
                | IrcMessage(sender, _, [user; modes]) -> Some(sender, user, modes)
                | _ -> None
        else None

    let (|SERVICE|_|) (message: IrcMessage) =
        if message.Command %= "service" then
            match message with
                | IrcMessage(sender, _, [nickname; distribution; ``type``; info]) -> Some(sender, nickname, distribution, ``type``, info)
                | _ -> None
        else None

    let (|QUIT|_|) (message: IrcMessage) =
        if message.Command %= "quit" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [message]) -> Some(sender, Some(message))
                | _ -> None
        else None

    let (|SQUIT|_|) (message: IrcMessage) =
        if message.Command %= "squit" then
            match message with
                | IrcMessage(sender, _, [server; comment]) -> Some(sender, server, comment)
                | _ -> None
        else None

    let (|JOIN|_|) (message: IrcMessage) =
        if message.Command %= "join" then
            match message with
                | IrcMessage(sender, _, [channels]) -> Some(sender, channels, None)
                | IrcMessage(sender, _, [channels; keys]) -> Some(sender, channels, Some(keys))
                | _ -> None
        else None

    let (|PART|_|) (message: IrcMessage) =
        if message.Command %= "part" then
            match message with
                | IrcMessage(sender, _, [channels]) -> Some(sender, channels, None)
                | IrcMessage(sender, _, [channels; message]) -> Some(sender, channels, Some(message))
                | _ -> None
        else None

    let (|TOPIC|_|) (message: IrcMessage) =
        if message.Command %= "topic" then
            match message with
                | IrcMessage(sender, _, [channel]) -> Some(sender, channel, None)
                | IrcMessage(sender, _, [channel; topic]) -> Some(sender, channel, Some(topic))
                | _ -> None
        else None

    let (|NAMES|_|) (message: IrcMessage) =
        if message.Command %= "names" then
            match message with
                | IrcMessage(sender, _, [channels]) -> Some(sender, channels, None)
                | IrcMessage(sender, _, [channels; target]) -> Some(sender, channels, Some(target))
                | _ -> None
        else None

    let (|LIST|_|) (message: IrcMessage) =
        if message.Command %= "list" then
            match message with
                | IrcMessage(sender, _, [channels]) -> Some(sender, channels, None)
                | IrcMessage(sender, _, [channels; target]) -> Some(sender, channels, Some(target))
                | _ -> None
        else None

    let (|INVITE|_|) (message: IrcMessage) =
        if message.Command %= "invite" then
            match message with
                | IrcMessage(sender, _, [nickname; channel]) -> Some(sender, nickname, channel)
                | _ -> None
        else None

    let (|KICK|_|) (message: IrcMessage) =
        if message.Command %= "kick" then
            match message with
                | IrcMessage(sender, _, [channels; users]) -> Some(sender, channels, users)
                | _ -> None
        else None

    let (|PRIVMSG|_|) (message: IrcMessage) =
        if message.Command %= "privmsg" then
            match message with
                | IrcMessage(sender, _, [recipients; message]) -> Some(sender, recipients, message)
                | _ -> None
        else None

    let (|NOTICE|_|) (message: IrcMessage) =
        if message.Command %= "notice" then
            match message with
                | IrcMessage(sender, _, [msgtarget; text]) -> Some(sender, msgtarget, text)
                | _ -> None
        else None

    let (|MOTD|_|) (message: IrcMessage) =
        if message.Command %= "motd" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|LUSERS|_|) (message: IrcMessage) =
        if message.Command %= "lusers" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None, None)
                | IrcMessage(sender, _, [mask]) -> Some(sender, Some(mask), None)
                | IrcMessage(sender, _, [mask; target]) -> Some(sender, Some(mask), Some(target))
                | _ -> None
        else None

    let (|VERSION|_|) (message: IrcMessage) =
        if message.Command %= "version" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|STATS|_|) (message: IrcMessage) =
        if message.Command %= "stats" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None, None)
                | IrcMessage(sender, _, [query]) -> Some(sender, Some(query), None)
                | IrcMessage(sender, _, [query; target]) -> Some(sender, Some(query), Some(target))
                | _ -> None
        else None

    let (|LINKS|_|) (message: IrcMessage) =
        if message.Command %= "links" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None, None)
                | IrcMessage(sender, _, [remoteServer]) -> Some(sender, Some(remoteServer), None)
                | IrcMessage(sender, _, [remoteServer; serverMask]) -> Some(sender, Some(remoteServer), Some(serverMask))
                | _ -> None
        else None

    let (|TIME|_|) (message: IrcMessage) =
        if message.Command %= "time" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|CONNECT|_|) (message: IrcMessage) =
        if message.Command %= "connect" then
            match message with
                | IrcMessage(sender, _, [targetServer; port]) -> Some(sender, targetServer, port, None)
                | IrcMessage(sender, _, [targetServer; port; remoteServer]) -> Some(sender, targetServer, port, Some(remoteServer))
                | _ -> None
        else None

    let (|TRACE|_|) (message: IrcMessage) =
        if message.Command %= "trace" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|ADMIN|_|) (message: IrcMessage) =
        if message.Command %= "admin" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|INFO|_|) (message: IrcMessage) =
        if message.Command %= "info" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None)
                | IrcMessage(sender, _, [target]) -> Some(sender, Some(target))
                | _ -> None
        else None

    let (|SERVLIST|_|) (message: IrcMessage) =
        if message.Command %= "servlist" then
            match message with
                | IrcMessage(sender, _, []) -> Some(sender, None, None)
                | IrcMessage(sender, _, [mask]) -> Some(sender, Some(mask), None)
                | IrcMessage(sender, _, [mask; ``type``]) -> Some(sender, Some(mask), Some(``type``))
                | _ -> None
        else None

    let (|SQUERY|_|) (message: IrcMessage) =
        if message.Command %= "squery" then
            match message with
                | IrcMessage(sender, _, [servicename; text]) -> Some(sender, servicename, text)
                | _ -> None
        else None

    let (|WHO|_|) (message: IrcMessage) =
        if message.Command %= "who" then
            match message with
                | IrcMessage(sender, _, [mask]) -> Some(sender, mask, None)
                | IrcMessage(sender, _, [mask; operatorsOnly]) -> Some(sender, mask, Some(operatorsOnly))
                | _ -> None
        else None

    let (|WHOIS|_|) (message: IrcMessage) =
        if message.Command %= "whois" then
            match message with
                | IrcMessage(sender, _, [masks]) -> Some(sender, masks, None)
                | IrcMessage(sender, _, [target; masks]) -> Some(sender, target, Some(masks))
                | _ -> None
        else None

    let (|WHOWAS|_|) (message: IrcMessage) =
        if message.Command %= "whowas" then
            match message with
                | IrcMessage(sender, _, [nicknames]) -> Some(sender, nicknames, None, None)
                | IrcMessage(sender, _, [nicknames; count]) -> Some(sender, nicknames, Some(count), None)
                | IrcMessage(sender, _, [nicknames; count; target]) -> Some(sender, nicknames, Some(count), Some(target))
                | _ -> None
        else None

    let (|KILL|_|) (message: IrcMessage) =
        if message.Command %= "kill" then
            match message with
                | IrcMessage(sender, _, [nickname; comment]) -> Some(sender, nickname, comment)
                | _ -> None
        else None

    let (|PING|_|) (message: IrcMessage) =
        if message.Command %= "ping" then
            match message with
                | IrcMessage(sender, _, [server1]) -> Some(sender, server1, None)
                | IrcMessage(sender, _, [server1; server2]) -> Some(sender, server1, Some(server2))
                | _ -> None
        else None

    let (|PONG|_|) (message: IrcMessage) =
        if message.Command %= "pong" then
            match message with
                | IrcMessage(sender, _, [server1]) -> Some(sender, server1, None)
                | IrcMessage(sender, _, [server1; server2]) -> Some(sender, server1, Some(server2))
                | _ -> None
        else None