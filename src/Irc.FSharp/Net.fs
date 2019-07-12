namespace Irc.FSharp

open System
open System.IO
open System.Net
open System.Net.Security
open System.Net.Sockets
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates

type IrcClient private (client: TcpClient, dataStream: Stream, ?certCallback: RemoteCertificateValidationCallback, ?msgEvent: Event<IrcMessage>) as this = 

    let mutable disposed = false

    let mutable lastActionTime = DateTime.Now
    let mutable lastPingTime = DateTime.MinValue
    
    let reader = new StreamReader(dataStream) |> TextReader.Synchronized
    let writer = 
        new StreamWriter(dataStream, AutoFlush = true) 
        |> TextWriter.Synchronized
    
    let messageEvent = defaultArg msgEvent (Event<IrcMessage>())

    let handleMessageInternal message =
        let now = DateTime.UtcNow
        lastActionTime <- now

        match message with
        | PING(_, value, _) -> 
            IrcMessage.pong value |> this.WriteMessage
            lastPingTime <- now
        | PONG(_, value, _) ->
            lastPingTime <- now
        | _ -> ()
        
    let readMessage () = 
        async { 
            let! line = reader.ReadLineAsync() |> Async.AwaitTask
            
            // parse result can somehow end up being null if trying to read an ssl stream as plaintext
            match IrcMessage.TryParse line with
            | Ok message ->
                handleMessageInternal message
                return message
            | Result.Error e ->
                // TODO: handle non-fatal parse errors
                return raise e
        }     
    
    let readerAgent = MailboxProcessor<IrcMessage>.Start(fun mbox ->
        let rec loop () =
            async { 
                if this.Connected then
                    let! msg = readMessage()
                    messageEvent.Trigger msg
                    return! loop()
            }
        loop ())

    new (host: string, port: int, ?ssl: bool, ?validateCertCallback: RemoteCertificateValidationCallback) = 
        let client = new TcpClient(host, port)
        
        let dataStream: Stream = 
            if defaultArg ssl false then
                upcast new SslStream(client.GetStream(), true, defaultArg validateCertCallback noSslErrors)
            else
                upcast client.GetStream()

        if dataStream :? SslStream then 
            (dataStream :?> SslStream)
                .AuthenticateAsClient(
                    host,
                    X509CertificateCollection(), 
                    SslProtocols.None,
                    true)

        new IrcClient(client, dataStream, defaultArg validateCertCallback noSslErrors)

    new (host: string, port: int, ssl: bool, validateCertPredicate) =
        new IrcClient(host, port, ssl, validateCertCallback=(validateCertCallback validateCertPredicate))
    
    new(host: IPAddress, port, ?ssl, ?validateCertCallback) =
        new IrcClient(host.ToString(), port, defaultArg ssl false, defaultArg validateCertCallback noSslErrors)

    new(endPoint: EndPoint, ?ssl, ?validateCertCallback) =
        let host, port = 
            match endPoint with
            | :? IPEndPoint as ip ->
                string ip.Address, ip.Port
            | :? DnsEndPoint as name ->
                name.Host, name.Port
        new IrcClient(host, port, defaultArg ssl false, defaultArg
            validateCertCallback noSslErrors)
    
    static member ConnectAsync(host: string, port: int, ?ssl: bool, ?validateCertCallback: RemoteCertificateValidationCallback) = 
        async { 
            let client = new TcpClient()
            do! client.ConnectAsync(host, port)
                |> Async.AwaitIAsyncResult
                |> Async.Ignore

            let dataStream: Stream = 
                if defaultArg ssl false then 
                    upcast new SslStream(client.GetStream(), true, defaultArg validateCertCallback noSslErrors)
                else 
                    upcast client.GetStream()

            if dataStream :? SslStream then 
                do! (dataStream :?> SslStream).AuthenticateAsClientAsync(
                        host,
                        X509CertificateCollection(),
                        SslProtocols.None,
                        true)
                    |> Async.AwaitIAsyncResult
                    |> Async.Ignore

            return new IrcClient(client, dataStream, defaultArg validateCertCallback noSslErrors)
        }

    static member ConnectAsync(endpoint: EndPoint, ?ssl: bool, ?validateCertCallback: RemoteCertificateValidationCallback) =
        let host, port =
            match endpoint with
            | :? IPEndPoint as ip -> string ip.Address, ip.Port
            | :? DnsEndPoint as dns -> dns.Host, dns.Port
            
        IrcClient.ConnectAsync(host, port,
            defaultArg ssl false,
            defaultArg validateCertCallback noSslErrors)

    member internal this.IsSsl = dataStream :? SslStream

    member this.LastMessageTime
        with get() = lastActionTime
        and private set (value) = lastActionTime <- value

    member this.LastPingTime
        with get() = lastPingTime
        and private set (value) = lastPingTime <- value

    member this.RemoteEndPoint = client.Client.RemoteEndPoint

    member this.LocalEndPoint = client.Client.LocalEndPoint

    member this.ReadTimeout
        with get () = client.ReceiveTimeout
        and set v = client.ReceiveTimeout <- v
    
    [<CLIEvent>]
    member this.MessageReceived = messageEvent.Publish

    [<CLIEvent>]
    member this.Error = readerAgent.Error
                
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

    member internal this.ReconnectAsync() =
        async { 
            let host, port = 
                match client.Client.RemoteEndPoint with
                | :? DnsEndPoint as ep -> ep.Host, ep.Port
                | :? IPEndPoint as ep -> string ep.Address, ep.Port

            let remoteCertSubject = 
                match dataStream with
                | :? SslStream as sslStream -> 
                    use cert = new X509Certificate2(sslStream.RemoteCertificate)
                    Some(cert.GetNameInfo(X509NameType.DnsName, false))
                | _ -> None

            do (this :> IDisposable).Dispose()

            let newClient = new TcpClient()
            do! client.ConnectAsync(host, port)
                |> Async.AwaitIAsyncResult
                |> Async.Ignore

            let newStream: Stream = 
                if dataStream :? SslStream then
                    upcast new SslStream(newClient.GetStream(), true, defaultArg certCallback noSslErrors)
                else
                    upcast client.GetStream()

            if newStream :? SslStream then 
                do! 
                    (newStream :?> SslStream).AuthenticateAsClientAsync(
                        (Option.get remoteCertSubject),
                        X509CertificateCollection(), 
                        SslProtocols.None, true)
                    |> Async.AwaitIAsyncResult
                    |> Async.Ignore                    
                        
            return new IrcClient(newClient, newStream, defaultArg certCallback noSslErrors, messageEvent) 

        }

    member this.Reconnect() =
        this.ReconnectAsync ()
        |> Async.RunSynchronously

    interface IDisposable with
        override this.Dispose() = 
            do disposed <- true
            client.Close()
            dispose [ reader; writer; readerAgent ]

            match dataStream with
            | :? SslStream as sslStream -> 
                (sslStream :> IDisposable).Dispose()
            | _ -> ()
