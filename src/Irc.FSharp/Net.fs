namespace Irc.FSharp

open System
open System.IO
open System.Net.Security
open System.Net.Sockets
open System.Threading
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates

[<AutoOpen>]
module private Utilities = 

    let inline objDisposed< ^T> = raise (ObjectDisposedException typeof< ^T>.FullName)

    let inline dispose(garbage: seq<IDisposable>) = garbage |> Seq.iter(fun disposable -> disposable.Dispose())

    let inline objDisposedIf< ^T> predicate = if predicate then objDisposed< ^T>

    let noSslErrors = new RemoteCertificateValidationCallback(fun _ _ _ errors -> errors = SslPolicyErrors.None)

    let validateCertCallback cb = new RemoteCertificateValidationCallback(cb)
        
type IrcClient private (server: string, port: int, client: TcpClient, dataStream: Stream) as this = 
    let mutable disposed = false

    let mutable lastActionTime = DateTime.Now
    let mutable lastPingTime = DateTime.Now
    
    let reader = new StreamReader(dataStream) |> TextReader.Synchronized
    let writer = new StreamWriter(dataStream, AutoFlush = true) |> TextWriter.Synchronized
    
    let msgEvent = Event<IrcMessage>()

    let internalOnMessage message =
        let now = DateTime.UtcNow
        lastActionTime <- now

        match message with
        | PING(_, value, _) -> 
            IrcMessage.pong value |> this.WriteMessage
            this.LastPingTime <- now
        | _ -> ()
        
    let readMessage () = 
        async { 
            let! line = reader.ReadLineAsync() |> Async.AwaitTask
            match IrcMessage.TryParse line with
            | Ok message ->
                internalOnMessage message
                return message
            | Result.Error parseErr -> 
                // TODO: handle non-fatal parse errors
                return raise (Exception (parseErr.Messages.ToString()))
        }     
    
    let readerAgent = MailboxProcessor<IrcMessage>.Start(fun mbox ->
        let rec loop () =
            async { 
                if this.Connected then
                    let! msg = readMessage()
                    msgEvent.Trigger msg
                    return! loop()
            }
        loop ())
 
    new (server: string, port: int, ?ssl: bool, ?validateCertFunction: RemoteCertificateValidationCallback) = 
        let client = new TcpClient(server, port)
        
        let dataStream: Stream = 
            if defaultArg ssl false then
                upcast new SslStream(client.GetStream(), true, defaultArg validateCertFunction noSslErrors)
            else
                upcast client.GetStream()

        if dataStream :? SslStream then 
            (dataStream :?> SslStream)
                .AuthenticateAsClient(
                    server,
                    new X509CertificateCollection(), 
                    SslProtocols.Default ||| SslProtocols.Tls12,
                    true)

        new IrcClient(server, port, client, dataStream)

    new (server: string, port: int, ssl: bool, validateCertPredicate) =
        new IrcClient(server, port, ssl, validateCertFunction=(validateCertCallback validateCertPredicate))
    
    static member ConnectAsync(server: string, port: int, ?ssl: bool, ?validateCertFunction: RemoteCertificateValidationCallback) = 
        async { 
            let client = new TcpClient()
            do! client.ConnectAsync(server, port)
                |> Async.AwaitIAsyncResult
                |> Async.Ignore

            let dataStream: Stream = 
                if defaultArg ssl false then 
                    upcast new SslStream(client.GetStream(), true, defaultArg validateCertFunction noSslErrors)
                else 
                    upcast client.GetStream()

            if dataStream :? SslStream then 
                do! (dataStream :?> SslStream).AuthenticateAsClientAsync(
                        server,
                        new X509CertificateCollection(),
                        SslProtocols.Default ||| SslProtocols.Tls12,
                        true)
                    |> Async.AwaitIAsyncResult
                    |> Async.Ignore

            return new IrcClient(server, port, client, dataStream)
        }

    member this.LastMessageTime
        with get() = lastActionTime
        and private set (value) = lastActionTime <- value

    member this.LastPingTime
        with get() = lastPingTime
        and private set (value) = lastPingTime <- value

    member this.Server = server

    member this.Port = port
    
    [<CLIEvent>]
    member this.MessageReceived = msgEvent.Publish
        
    member this.NextMessage () =
        objDisposedIf<IrcClient> disposed
        Async.AwaitEvent (this.MessageReceived)
                
    member this.Connected = 
        objDisposedIf<IrcClient> disposed
        client.Client.Connected

    member this.WriteMessage(message: IrcMessage) = 
        objDisposedIf<IrcClient> disposed
        message.ToString()
        |> writer.WriteLine

    member this.WriteMessageAsync(message: IrcMessage) = 
        objDisposedIf<IrcClient> disposed
        async { 
            do! writer.WriteLineAsync(message.ToString())
                |> Async.AwaitIAsyncResult
                |> Async.Ignore
        }

    interface IDisposable with
        override this.Dispose() = 
            do disposed <- true
               client.Close()
               dispose [ reader; writer; readerAgent ]

               match dataStream with
               | :? SslStream as sslStream -> (sslStream :> IDisposable).Dispose()
               | _ -> ()
