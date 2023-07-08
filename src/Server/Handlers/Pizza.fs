module FunPizzaShop.Server.Handlers.Pizza

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

let pizzaAPI (ctx: HttpContext) (env: #_) : API.Order = {
    OrderPizza =
        fun order  ->
            async {
                let pizzaService = env :> IPizza
                do! pizzaService.Order order
                return ()
            }
}

let pizzaHandler (env: #_) =
    Remoting.createApi ()
    |> Remoting.withErrorHandler (fun ex routeInfo -> Log.Error(ex,"Remoting error");  Propagate ex.Message; )
    |> Remoting.withRouteBuilder API.Route.builder
    |> Remoting.fromContext (fun ctx -> pizzaAPI ctx env)
    |> Remoting.buildHttpHandler
