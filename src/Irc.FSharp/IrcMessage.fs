namespace Irc.FSharp

open System
open System.Collections.Generic
open System.Text

open Microsoft.FSharp.Core.Printf

type IrcPrefix = 
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

[<AutoOpen>]
module IrcPrefix =

    let (|Nickname|_|) =
        function
        | User(nick, _, _) -> Some nick
        | _ -> None

    let (|Username|_|) =
        function
        | User(_, Some uname, _) -> Some uname
        | _ -> None

    let (|Hostmask|_|) =
        function
        | User(_, _, Some hostmask) -> Some hostmask
        | _ -> None

type IrcMessage = {
    Tags: IDictionary<string, string option>
    Prefix: IrcPrefix
    Command: string
    Arguments: string list
} with
    override this.ToString() = 
        (* let concatTags tags =
            let rec loop tags = *)
        let concatMessageArgs args = 
            let sb = StringBuilder()
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

        match this.Arguments with
        | [] ->
            sprintf "%s%s" prefix this.Command
        | args -> 
            sprintf "%s%s %s" prefix this.Command (concatMessageArgs args)

    static member Create(prefix, command, args, ?tags) =
        { Tags = defaultArg tags (dict [])
          Prefix = prefix
          Command = command
          Arguments = args }