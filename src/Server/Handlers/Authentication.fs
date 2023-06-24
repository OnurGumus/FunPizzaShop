module FunPizzaShop.Server.Handlers.Authentication


open FunPizzaShop.Server.Views
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open System.Threading.Tasks
open FunPizzaShop.Domain.Model.Authentication
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Antiforgery
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FunPizzaShop.Domain
open Microsoft.Extensions.Configuration
open FunPizzaShop.Server.Command
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model
open System.IO
open System.Collections.Generic
open Serilog

let lines = [] //File.ReadAllLines("email.txt")
let emails = HashSet<string>(lines)

[<AutoOpen>]
module private Internal =

    let prepareClaimsPrincipal name admins =

        let claims = [ Claim(ClaimTypes.Name, name) ]

        let claims =
            if admins |> Set.contains name then
                Claim(ClaimTypes.Role, "admin") :: claims
            else
                claims

        ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
        |> ClaimsPrincipal


let authenticationAPI (ctx: HttpContext) (env: #_) : API.Authentication = {
    Login =
        fun userId  ->
            async {
                let email = userId.Value

                let anyError =
                    if email.Contains("@") then
                        let domainName = email.Split('@').[1]

                        if (emails.Contains(domainName)) then
                            Some(Error [ LoginError "Please use work email to login!" ])
                        else
                            None
                    else
                        None

                if anyError.IsSome then
                    return anyError.Value
                else
                    let auth = env :> IAuthentication
                    let! result = auth.Login userId
                    return result
            }
    Verify =
        fun (userId, verificationCode) ->
            async {
                let auth = env :> IAuthentication
                let config = env :> IConfiguration
                let! result = auth.Verify(userId, verificationCode)

                let admins =
                    config.GetSection("config:admins").AsEnumerable()
                    |> Seq.map (fun x -> x.Value)
                    |> Seq.filter (isNull >> not)
                    |> Set.ofSeq

                match result with
                | Ok _ ->
                    let p = prepareClaimsPrincipal userId.Value admins
                    do! ctx.SignInAsync(p) |> Async.AwaitTask
                | _ -> ()

                return result
            }
    Logout =
        fun userId ->
            async {
                let auth = env :> IAuthentication
                let! result = auth.Logout userId

                match result with
                | Ok _ -> do! ctx.SignOutAsync() |> Async.AwaitTask
                | _ -> ()

                return result
            }
}

let authenticationHandler (env: #_) =
    Remoting.createApi ()
    |> Remoting.withErrorHandler (fun ex routeInfo -> Log.Error(ex,"Remoting error");  Propagate ex.Message; )
    |> Remoting.withRouteBuilder API.Route.builder
    |> Remoting.fromContext (fun ctx -> authenticationAPI ctx env)
    |> Remoting.buildHttpHandler

open Google.Apis.Auth
open Google.Apis.Auth.OAuth2
open Serilog
// etc.
let googleSignIn (env: #_) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            Log.Information("google login start")
            let token = ctx.Request.Form.["credential"][0]
            Log.Information("google login token: {token}", token)
            let! payload = GoogleJsonWebSignature.ValidateAsync(token)
            let auth = env :> IAuthentication
            let config = env :> IConfiguration
            let email = payload.Email
            let name = payload.Name
            let userId = email |> UserId.TryCreate |> forceValidate
            let! result = auth.Verify(userId, None)

            match result with
            | Ok _ ->
                let p = prepareClaimsPrincipal userId.Value Set.empty
                do! ctx.SignInAsync(p) |> Async.AwaitTask
            | _ -> ()

            Log.Information("google login done: {email} {name}", email, name)

            ctx.SetHttpHeader("Location", "https://mydomain.com")
            return! setStatusCode 303 earlyReturn ctx
        }
