namespace Irc.FSharp

open System
open System.Net.Security

[<AutoOpen>]
module internal Utils =

    let inline split (str: string) (separator: char) =
        str.Split([|separator|], StringSplitOptions.RemoveEmptyEntries)

    let inline equalsCI (str1: string) (str2: string) = str1.Equals(str2, System.StringComparison.CurrentCultureIgnoreCase)

    let inline dispose(garbage: seq<IDisposable>) = garbage |> Seq.iter(fun disposable -> disposable.Dispose())

    let inline objDisposed< ^T> = raise (ObjectDisposedException typeof< ^T>.FullName)

    let inline objDisposedIf< ^T> predicate = if predicate then objDisposed< ^T>

    let inline validateCertCallback cb = new RemoteCertificateValidationCallback(cb)

    let noSslErrors = validateCertCallback (fun _ _ _ errors -> errors = SslPolicyErrors.None)

    let noValidation = validateCertCallback (fun _ _ _ _ -> true)

    let (|StrCons|_|) (needle: string) (pattern: string) =
        match pattern.IndexOf needle with
        | -1 -> None
        | n -> Some <| pattern.[n + needle.Length..].TrimStart()

    let (|StrConsCI|_|) (needle: string) (pattern: string) =
        match pattern.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) with
        | -1 -> None
        | n -> Some <| pattern.[n + needle.Length..].TrimStart()



    
