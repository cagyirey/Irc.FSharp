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

