namespace Irc.FSharp

open Irc.FSharp.Parser

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IrcMessage = 

    let withPrefix (prefix: IrcRecipient) (message: IrcMessage) =
        match message with
        | IrcMessage(_, cmd, args) -> IrcMessage(prefix, cmd, args)

    let (|NumericResponse|_|) (responseCode: int) (message: IrcMessage) =
        match System.Int32.TryParse message.Command with
        | true, x when x = responseCode -> Some ()
        | _, _ -> None

    type IrcRecipient with
        static member ParseMany(value) =
            (Parser.parseRecipients >> Parser.unboxParserResult) value

        static member TryParseMany(value) =
            (Parser.parseRecipients >> Parser.tryUnboxParserResult) value

        static member Parse(value) =
            (Parser.parseRecipient >> Parser.unboxParserResult) value

        static member TryParse(value) =
            (Parser.parseRecipient >> Parser.tryUnboxParserResult) value

    type IrcMessage with
        static member Parse(value) =
            (Parser.parseIrcMessage >> Parser.unboxParserResult) value

        static member TryParse(value) =
            (Parser.parseIrcMessage >> Parser.tryUnboxParserResult) value