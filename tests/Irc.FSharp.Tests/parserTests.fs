namespace Irc.FSharp

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
            |> should equal (Some "nickname")
           
        [<Test>]
        let ``Can match a username pattern`` () =
            ``|Username|_|`` userPrefix
            |> should equal (Some "username")

        [<Test>]
        let ``Can match a hostmask pattern`` () =
            ``|Hostmask|_|`` userPrefix
            |> should equal (Some "hostmask")

    module ``IRC message parsing tests`` =

        [<Test>]
        let ``Can construct a PRIVMSG with the IrcMessage union type`` () =
            let message = 
                IrcMessage.Create(
                    User("nickname", Some "username", Some "host.mask"),
                    "PRIVMSG",
                    ["#channel1"; "hello"])

            message |> should equal (IrcMessage.Parse rawPrivmsg)

        [<Test>]
        let ``Can construct a PRIVMSG with the privmsg function`` () =
            let message = 
                { IrcMessage.privmsg ["#channel1"] "hello" 
                    with
                        Prefix = (User("nickname", Some "username", Some "host.mask"))
                }                

            message |> should equal (IrcMessage.Parse rawPrivmsg)