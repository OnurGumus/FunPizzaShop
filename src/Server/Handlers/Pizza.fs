module FunPizzaShop.Server.Handlers.Pizza

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

let pizzaAPI (ctx: HttpContext) (env: #_) : API.Order = {
    OrderPizza =
        fun order  ->
            async {
                return ()
            }
}

let pizzaHandler (env: #_) =
    Remoting.createApi ()
    |> Remoting.withErrorHandler (fun ex routeInfo -> Log.Error(ex,"Remoting error");  Propagate ex.Message; )
    |> Remoting.withRouteBuilder API.Route.builder
    |> Remoting.fromContext (fun ctx -> pizzaAPI ctx env)
    |> Remoting.buildHttpHandler
