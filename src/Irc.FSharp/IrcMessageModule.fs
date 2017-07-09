namespace Irc.FSharp

open Irc.FSharp.Parser

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IrcMessage = 

    let withPrefix (prefix: IrcPrefix) (message: IrcMessage) =
        match message with
        | IrcMessage(_, cmd, args) -> IrcMessage(prefix, cmd, args)

    let (|NumericResponse|_|) (responseCode: int) (message: IrcMessage) =
        match System.Int32.TryParse message.Command with
        | true, x when x = responseCode -> Some message.Arguments
        | _, _ -> None

    type IrcPrefix with
        static member ParseMany =
            parsePrefixes >> unboxParserResult

        static member TryParseMany =
            parsePrefixes >> tryUnboxParserResult

        static member Parse =
            parsePrefix >> unboxParserResult

        static member TryParse =
            parsePrefix >> tryUnboxParserResult

    type IrcMessage with
        static member Parse =
            parseIrcMessage >> unboxParserResult

        static member TryParse =
            parseIrcMessage >> tryUnboxParserResult