namespace Irc.FSharp

open FParsec.CharParsers
open FParsec.Primitives

module internal Parser =

    [<AutoOpen>]
    module internal Internal = 
        let word = many1Satisfy ((<>) ' ')

        let inline isSpecial c = 
            uint32 c - uint32 '[' <= uint32 '`' - uint32 '[' || uint32 c - uint32 '{' <= uint32 '}' - uint32 '{'

        let inline isLetterOrSpecial c =
            isLetter c || isSpecial c 

        let inline isAlphanumOrSpecial c =
            isLetterOrSpecial c || isDigit c

        // strict irc grammar for nickSegment: many1Satisfy2 (isAlphanumOrSpecial) (isNoneOf "!@. ") .>> (nextCharSatisfies ((<>) '.') <|> eof)
        let nickSegment = many1Chars (noneOf " .:!@\r\n") .>> (nextCharSatisfies ((<>) '.') <|> eof) 
        let userSegment = skipChar '!' >>. many1Satisfy(isNoneOf "@ ")
        let hostSegment = skipChar '@' >>. word

        let userRecipient = tuple3 (nickSegment) (opt userSegment) (opt hostSegment) |>> User
        let channelRecipient = (many1Chars2 (anyOf "#&+!") (noneOf "\x00\a\r\n, ")) |>> Channel
        let serverRecipient = word |>> Server

        let recipient = choice [|attempt channelRecipient; attempt userRecipient; attempt serverRecipient;|]
        let recipients = sepBy1 recipient (skipChar ',') .>> spaces1

        let messagePrefix = skipChar ':' >>. recipient .>> spaces1
        let ircCommand = many1Chars asciiLetter <|> manyMinMaxSatisfy 3 3 isDigit .>> spaces
        let middleParam = nextCharSatisfies ((<>) ':') >>. word .>> spaces
        let tailParam = skipChar ':' >>. restOfLine false
        let paramArray = many (middleParam <|> tailParam)
        let message = tuple3 (opt messagePrefix |>> Option.fold (fun _ prefix -> prefix) Empty) ircCommand paramArray |>> IrcMessage

    let internal tryUnboxParserResult result = 
        match result with
        | ParserResult.Success(res, _, _) -> Result.Ok res
        | ParserResult.Failure(_, err, _) -> Result.Error (err.Messages.ToString() |> exn)

    let internal unboxParserResult result =
        match result with
        | ParserResult.Success(r, _, _) -> r
        | ParserResult.Failure(reason, _, _) -> invalidArg "result" reason
            
    let parseIrcMessage str =
        run Internal.message str

    let parseRecipient str =
        run Internal.recipient str

    let parseRecipients str =
        run Internal.recipients str
