### 0.5.0 - July 12 2019
* Updated README
* Forwarded remaining `IrcClient` members, making them visible from `IrcConnection` API.
* Fixed `IrcConnection.SendMessage`/`IrcConnection.SendMessageAsync` to appear as a method instead of a property.

### 0.4.0 - July 12 2019
* Added an `IrcClient.ConnectAsync` constructor taking an `EndPoint`.
* `IrcClient.ReadTimeout` is now in milliseconds.
* Removed asynchronous `IrcClient.NextMessage` method.
* Added a graceful reconnect feature in `IrcClient.ReconnectAsync`/`Irc.ClientReconnect` and `IrcConnection.ReconnectAsync`/`IrcClient.Reconnect`.
* `IrcConnection.OnReady` is now correctly marked as `internal`.
* `MessageReceived`, `SendMessage` and `SendMessageAsync` are now usable from the `IrcConnection` type.
* Unit tests are correctly run at build time.

### 0.3.0 - May 25 2019
* Valid TLS protocols are now determined by the OS.
* Improved naming conventions for `IrcConnection` members.
* Target framework changed to .NET Core 2.2.
* Upgraded to FAKE 5 and improved build automation scripts.

### 0.2.0 - July 9 2017
* `NumericResponse` takes a numeric string now, and decomposes a response into its parameters
* Added a `ReadTimeout` property to `IrcClient`
* Added an experimental `IrcConnection` type encapsulating the dynamic state of the client and server
* Added some nonstandard response codes
* Added new constructors for `IrcClient` taking an `IPAddress` or `EndPoint`
* Added support for IRCv3 capability negotiation (via `IrcConnection`)

### 0.1.1 - May 6 2016
* Fixes to unit testing
* Fixes to pattern matching

### 0.1.0 - May 5 2017
* Clean up Paket dependencies
* Change `IrcRecipient` to `IrcPrefix`
* Added pattern matching for nickname, user and hostmasks
* Added unit tests

### 0.0.1 - April 27 2017
* Initial release

