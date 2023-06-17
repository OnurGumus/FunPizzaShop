module FunPizzaShop.Server.Throttling

open ThrottlingTroll

let setOptions (options: ThrottlingTrollOptions) =
    let config = ThrottlingTrollConfig()

    config.Rules <- [|
        ThrottlingTrollRule(
            UriPattern = "/api/Authentication/Login",
            LimitMethod = FixedWindowRateLimitMethod(PermitLimit = 5, IntervalInSeconds = 60),
            IdentityIdExtractor =
                fun (request) ->
                    let r = (request :?> IIncomingHttpRequestProxy).Request
                    r.HttpContext.Connection.RemoteIpAddress.ToString()
        )
        ThrottlingTrollRule(
            UriPattern = "/api/Authentication/Login",
            LimitMethod = FixedWindowRateLimitMethod(PermitLimit = 7, IntervalInSeconds = 600),
            IdentityIdExtractor =
                fun (request) ->
                    let r = (request :?> IIncomingHttpRequestProxy).Request
                    r.HttpContext.Connection.RemoteIpAddress.ToString()
        )
        ThrottlingTrollRule(
            UriPattern = "/api/Authentication/Verify",
            LimitMethod = FixedWindowRateLimitMethod(PermitLimit = 7, IntervalInSeconds = 60),
            IdentityIdExtractor =
                fun (request) ->
                    let r = (request :?> IIncomingHttpRequestProxy).Request
                    r.HttpContext.Connection.RemoteIpAddress.ToString()
        )
        ThrottlingTrollRule(
            UriPattern = "/api/Authentication/Verify",
            LimitMethod = FixedWindowRateLimitMethod(PermitLimit = 15, IntervalInSeconds = 600),
            IdentityIdExtractor =
                fun (request) ->
                    let r = (request :?> IIncomingHttpRequestProxy).Request
                    r.HttpContext.Connection.RemoteIpAddress.ToString()
        )
    |]


    options.Config <- config
