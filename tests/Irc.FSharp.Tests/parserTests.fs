namespace Irc.FSharp

open Irc.FSharp

open NUnit.Framework
open FsUnit

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Tests = 

    let rawPrivmsg = ":nickname!username@host.mask PRIVMSG #channel1 :hello"
    
    let userPrefix = User("nickname", Some "username", Some "hostmask")

    let channelPrefix = Channel "#channel1"

    let serverPrefix = Server "irc.example.com"

    module ``IRC message pattern matching tests`` =

        [<Test>]
        let ``Can match a nickname pattern`` () =
            ``|Nickname|_|`` userPrefix
            |> should equal "nickname"
            
        [<Test>]
        let ``Can match a username pattern`` () =
            ``|Nickname|_|`` userPrefix
            |> should equal "username"

        [<Test>]
        let ``Can match a hostmask pattern`` () =
            ``|Nickname|_|`` userPrefix
            |> should equal "hostmask"

    module ``IRC message parsing tests`` =

        [<Test>]
        let ``Can parse a PRIVMSG`` () =
            let message = 
                IrcMessage(
                    User("nickname", Some "username", Some "hostmask"),
                    "PRIVMSG",
                    ["channel1"; "hello"])

            message |> should equal rawPrivmsg

        [<Test>]
        let ``Can construct a PRIVMSG`` () =
            let message = 
                IrcMessage.privmsg ["channel1"] "hello"
                |> IrcMessage.withPrefix (User("nickname", Some "username", Some "hostmask"))

            message |> should equal rawPrivmsg