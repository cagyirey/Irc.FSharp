namespace Irc.FSharp

open System
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
    Tags: Map<string, string option>
    Prefix: IrcPrefix
    Command: string
    Arguments: string list
} with
    override this.ToString() = 
        let sb = StringBuilder()
        
        let appendTags tags =
            let rec loop tags =
                match tags with
                | [(key, None)] ->
                    bprintf sb "%s " key
                | [(key, Some value)] ->
                    bprintf sb "%s=%s " key value 
                | (key, None) :: rest ->
                    bprintf sb "%s;" key
                    loop rest
                | (key, Some value) :: rest ->
                    bprintf sb "%s=%s;" key value
                    loop rest
                | [] -> ()

            if not (Map.isEmpty tags) then
                sb.Append '@' |> ignore
                loop (Map.toList tags)              

        let appendMessageArgs args = 
            let rec loop args = 
                match args with
                | [ singleArg ] -> 
                    bprintf sb ":%s" singleArg
                | arg :: rest -> 
                    bprintf sb "%s " arg
                    loop rest
                | [] -> ()
            loop args

        appendTags this.Tags

        match this.Prefix with
        | Empty -> ()
        | pfx -> bprintf sb ":%O " pfx

        bprintf sb "%s " this.Command
        appendMessageArgs this.Arguments
        sb.ToString()

    static member Create(prefix, command, args, ?tags) =
        { Tags = defaultArg tags Map.empty
          Prefix = prefix
          Command = command
          Arguments = args }