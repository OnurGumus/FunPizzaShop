module FunPizzaShop.Server.Handlers.Authentication


open FunPizzaShop.Server.Views
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open System.Threading.Tasks
open FunPizzaShop.Shared.Model.Authentication
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Antiforgery
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FunPizzaShop.Shared
open Microsoft.Extensions.Configuration
open FunPizzaShop.ServerInterfaces.Command
open FunPizzaShop.ServerInterfaces.Query

open FunPizzaShop.Shared.Model
open System.IO
open System.Collections.Generic
open Serilog

val lines: 'a list
val emails: HashSet<string>

[<AutoOpen>]
module private Internal =
    val prepareClaimsPrincipal: name: string -> admins: Set<string> -> ClaimsPrincipal

val authenticationAPI:
    ctx: HttpContext -> env: 'a -> API.Authentication when 'a :> IAuthentication and 'a :> IConfiguration

val authenticationHandler:
    env: 'a -> (HttpFunc -> HttpContext -> HttpFuncResult) when 'a :> IAuthentication and 'a :> IConfiguration

open Google.Apis.Auth
open Google.Apis.Auth.OAuth2
open Serilog

val googleSignIn:
    env: 'a -> next: HttpFunc -> ctx: HttpContext -> HttpFuncResult when 'a :> IAuthentication and 'a :> IConfiguration
