namespace Irc.FSharp

open System
open System.Text

open Microsoft.FSharp.Core.Printf

type IrcRecipient = 
    | Channel of chstring: string
    | User of nick: string * user: string option * host: string option
    | Server of host: string
    | Empty

    override this.ToString() = 
        match this with
        // TODO: possibly use bprintf as this can be called often
        | Channel chstr -> sprintf "#%s" chstr
        | User(nick, user, host) -> 
            let user = Option.fold (fun _ u -> "!" + u) "" user
            let host = Option.fold (fun _ h -> "@" + h) "" host
            sprintf "%s%s%s" nick user host
        | Server hostname -> hostname
        | Empty -> String.Empty

type IrcMessage = 
    | IrcMessage of prefix: IrcRecipient * cmd: string * args: string list

    override this.ToString() = 
        let concatMessageArgs args = 
            let sb = new StringBuilder()
            let rec loop args = 
                match args with
                | [ singleArg ] -> 
                    bprintf sb ":%s" singleArg
                    sb.ToString()
                | arg :: rest -> 
                    bprintf sb "%s " arg
                    loop rest
                | [] -> sb.ToString()
            loop args

        let prefix =
            match this.Prefix with
            | Empty -> String.Empty
            | pfx -> sprintf ":%O " pfx

        match this with
        | IrcMessage(_, cmd, []) ->
            sprintf "%s%s" prefix cmd
        | IrcMessage(_, cmd, args) -> 
            sprintf "%s%s %s" prefix cmd (concatMessageArgs args)

    member this.Prefix : IrcRecipient = 
        match this with
        | IrcMessage(prefix, _, _) -> prefix

    member this.Command = 
        match this with
        | IrcMessage(_, cmd, _) -> cmd

    member this.Arguments = 
        match this with
        | IrcMessage(_, _, args) -> args
