module Irc.FSharp.ResponseCode

open System
open System.Reflection

let private responses = 
    query { 
        // TODO: reflect current type at start up
        for responseCode in Type.GetType("Irc.FSharp.ResponseCode, Irc.FSharp").GetTypeInfo().DeclaredFields do
            where (responseCode.IsLiteral && responseCode.FieldType = typeof<string>)
            select (responseCode.GetRawConstantValue() :?> string, responseCode.Name)
    }

let private responseNames = dict responses

let private responseCodes = 
    responses
    |> Seq.map (fun (code, name) -> name, code)
    |> dict

let getResponseName responseCode =
    match responseNames.TryGetValue responseCode with
    | true, responseName -> responseName
    | false, _ -> invalidArg "responseCode" "Invalid numeric response code."

let tryGetResponseName responseCode =
    match responseNames.TryGetValue responseCode with
    | true, responseName -> Some responseName
    | false, _ -> None

let getResponseCode responseName =
    match responseCodes.TryGetValue responseName with
    | true, responseCode -> responseCode
    | false, _ -> invalidArg "responseName" "Invalid response code name."

let tryGetResponseCode responseName =
    match responseCodes.TryGetValue responseName with
    | true, responseCode -> Some responseCode
    | false, _ -> None


[<Literal>]
let RPL_WELCOME = "001"

[<Literal>]
let RPL_YOURHOST = "002"

[<Literal>]
let RPL_CREATED = "003"

[<Literal>]
let RPL_MYINFO = "004"

[<Literal>]
let RPL_BOUNCE = "005"

[<Literal>]
let RPL_USERHOST = "302"

[<Literal>]
let RPL_ISON = "303"

[<Literal>]
let RPL_AWAY = "301"

[<Literal>]
let RPL_UNAWAY = "305"

[<Literal>]
let RPL_NOWAWAY = "306"

[<Literal>]
let RPL_WHOISUSER = "311"

[<Literal>]
let RPL_WHOISSERVER = "312"

[<Literal>]
let RPL_WHOISOPERATOR = "313"

[<Literal>]
let RPL_WHOISIDLE = "317"

[<Literal>]
let RPL_ENDOFWHOIS = "318"

[<Literal>]
let RPL_WHOISCHANNELS = "319"

[<Literal>]
let RPL_WHOWASUSER = "314"

[<Literal>]
let RPL_ENDOFWHOWAS = "369"

[<Literal>]
let RPL_LISTSTART = "321"

[<Literal>]
let RPL_LIST = "322"

[<Literal>]
let RPL_LISTEND = "323"

[<Literal>]
let RPL_UNIQOPIS = "325"

[<Literal>]
let RPL_CHANNELMODEIS = "324"

[<Literal>]
let RPL_NOTOPIC = "331"

[<Literal>]
let RPL_TOPIC = "332"

[<Literal>]
let RPL_INVITING = "341"

[<Literal>]
let RPL_SUMMONING = "342"

[<Literal>]
let RPL_INVITELIST = "346"

[<Literal>]
let RPL_ENDOFINVITELIST = "347"

[<Literal>]
let RPL_EXCEPTLIST = "348"

[<Literal>]
let RPL_ENDOFEXECPTLIST = "349"

[<Literal>]
let RPL_VERSION = "351"

[<Literal>]
let RPL_WHOREPLY = "352"

[<Literal>]
let RPL_ENDOFWHO = "315"

[<Literal>]
let RPL_NAMREPLY = "353"

[<Literal>]
let RPL_ENDOFNAMES = "366"

[<Literal>]
let RPL_LINKS = "364"

[<Literal>]
let RPL_ENDOFLINKS = "365"

[<Literal>]
let RPL_BANLIST = "367"

[<Literal>]
let RPL_ENDOFBANLIST = "368"

[<Literal>]
let RPL_INFO = "371"

[<Literal>]
let RPL_ENDOFINFO = "374"

[<Literal>]
let RPL_MOTDSTART = "375"

[<Literal>]
let RPL_MOTD = "372"

[<Literal>]
let RPL_ENDOFMOTD = "376"

[<Literal>]
let RPL_YOUREOPER = "381"

[<Literal>]
let RPL_REHASHING = "382"

[<Literal>]
let RPL_YOURESERVICE = "383"

[<Literal>]
let RPL_TIME = "391"

[<Literal>]
let RPL_USERSSTART = "392"

[<Literal>]
let RPL_USERS = "393"

[<Literal>]
let RPL_ENDOFUSERS = "394"

[<Literal>]
let RPL_NOUSERS = "395"

[<Literal>]
let RPL_TRACELINK = "200"

[<Literal>]
let RPL_TRACECONNECTING = "201"

[<Literal>]
let RPL_TRACEHANDSHAKE = "202"

[<Literal>]
let RPL_TRACEUKNOWN = "203"

[<Literal>]
let RPL_TRACEOPERATOR = "204"

[<Literal>]
let RPL_TRACEUSER = "205"

[<Literal>]
let RPL_TRACESERVER = "206"

[<Literal>]
let RPL_TRACESERVICE = "207"

[<Literal>]
let RPL_TRACENEWTYPE = "208"

[<Literal>]
let RPL_TRACECLASS = "209"

[<Literal>]
let RPL_TRACERECONNECT = "210"

[<Literal>]
let RPL_TRACELOG = "261"

[<Literal>]
let RPL_TRACEEND = "262"

[<Literal>]
let RPL_STATSLINKINFO = "211"

[<Literal>]
let RPL_STATSCOMMANDS = "212"

[<Literal>]
let RPL_ENDOFSTATS = "219"

[<Literal>]
let RPL_STATSUPTIME = "242"

[<Literal>]
let RPL_STATSOLINE = "243"

[<Literal>]
let RPL_UMODEIS = "221"

[<Literal>]
let RPL_SERVLIST = "234"

[<Literal>]
let RPL_SERVLISTEND = "235"

[<Literal>]
let RPL_LUSERCLIENT = "251"

[<Literal>]
let RPL_LUSEROP = "252"

[<Literal>]
let RPL_LUSERUNKNOWN = "253"

[<Literal>]
let RPL_LUSERCHANNELS = "254"

[<Literal>]
let RPL_LUSERME = "255"

[<Literal>]
let RPL_ADMINME = "256"

[<Literal>]
let RPL_ADMINLOC1 = "257"

[<Literal>]
let RPL_ADMINLOC2 = "258"

[<Literal>]
let RPL_ADMINEMAIL = "259"

[<Literal>]
let RPL_TRYAGAIN = "263"

[<Literal>]
let ERR_NOSUCHNICK = "401"

[<Literal>]
let ERR_NOSUCHSERVER = "402"

[<Literal>]
let ERR_NOSUCHCHANNEL = "403"

[<Literal>]
let ERR_CANNOTSENDTOCHAN = "404"

[<Literal>]
let ERR_TOOMANYCHANNELS = "405"

[<Literal>]
let ERR_WASNOSUCHNICK = "406"

[<Literal>]
let ERR_TOOMANYTARGETS = "407"

[<Literal>]
let ERR_NOSUCHSERVICE = "408"

[<Literal>]
let ERR_NOORIGIN = "409"

[<Literal>]
let ERR_NORECIPIENT = "411"

[<Literal>]
let ERR_NOTEXTTOSEND = "412"

[<Literal>]
let ERR_NOTOPLEVEL = "413"

[<Literal>]
let ERR_WILDTOPLEVEL = "414"

[<Literal>]
let ERR_BADMASK = "415"

[<Literal>]
let ERR_UNKNOWNCOMMAND = "421"

[<Literal>]
let ERR_NOMOTD = "422"

[<Literal>]
let ERR_NOADMININFO = "423"

[<Literal>]
let ERR_FILEERROR = "424"

[<Literal>]
let ERR_NONICKNAMEGIVEN = "431"

[<Literal>]
let ERR_ERRONEOUSNICKNAME = "432"

[<Literal>]
let ERR_NICKNAMEINUSE = "433"

[<Literal>]
let ERR_NICKCOLLISION = "436"

[<Literal>]
let ERR_UNAVAILRESOURCE = "437"

[<Literal>]
let ERR_USERNOTINCHANNEL = "441"

[<Literal>]
let ERR_NOTONCHANNEL = "442"

[<Literal>]
let ERR_USERONCHANNEL = "443"

[<Literal>]
let ERR_NOLOGIN = "444"

[<Literal>]
let ERR_SUMMONDISABLED = "445"

[<Literal>]
let ERR_USERSDISABLED = "446"

[<Literal>]
let ERR_NOTREGISTERED = "451"

[<Literal>]
let ERR_NEEDMOREPARAMS = "461"

[<Literal>]
let ERR_ALREADYREGISTRED = "462"

[<Literal>]
let ERR_NOPERMFORHOST = "463"

[<Literal>]
let ERR_PASSWDMISMATCH = "464"

[<Literal>]
let ERR_YOUREBANNEDCREEP = "465"

[<Literal>]
let ERR_YOUWILLBEBANNED = "466"

[<Literal>]
let ERR_KEYSET = "467"

[<Literal>]
let ERR_CHANNELISFULL = "471"

[<Literal>]
let ERR_UNKNOWNMODE = "472"

[<Literal>]
let ERR_INVITEONLYCHAN = "473"

[<Literal>]
let ERR_BANNEDFROMCHAN = "474"

[<Literal>]
let ERR_BADCHANNELKEY = "475"

[<Literal>]
let ERR_BADCHANMASK = "476"

[<Literal>]
let ERR_NOCHANMODES = "477"

[<Literal>]
let ERR_BANLISTFULL = "478"

[<Literal>]
let ERR_NOPRIVILEGES = "481"

[<Literal>]
let ERR_CHANOPRIVSNEEDED = "482"

[<Literal>]
let ERR_CANTKILLSERVER = "483"

[<Literal>]
let ERR_RESTRICTED = "484"

[<Literal>]
let ERR_UNIQOPPRIVSNEEDED = "485"

[<Literal>]
let ERR_NOOPERHOST = "491"

[<Literal>]
let ERR_UMODEUNKNOWNFLAG = "501"

[<Literal>]
let ERR_USERSDONTMATCH = "502"
