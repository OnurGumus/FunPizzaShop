module FunPizzaShop.Server.Handlers.Default

open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Authentication
open FunPizzaShop.Server.Handlers.Pizza
open Microsoft.AspNetCore.Authentication.Cookies
open FunPizzaShop.Shared.API

val webApp:
    env: 'a ->
    layout: (HttpContext -> (int -> Task<string>) -> Task<string>) ->
        (HttpFunc -> HttpContext -> HttpFuncResult)
        when 'a :> FunPizzaShop.ServerInterfaces.Query.IQuery
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IAuthentication
        and 'a :> Extensions.Configuration.IConfiguration
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IPizza

val webAppWrapper:
    env: 'a ->
    layout: (HttpContext -> (int -> Task<string>) -> Task<string>) ->
    next: HttpFunc ->
    context: HttpContext ->
        Task<HttpContext option>
        when 'a :> FunPizzaShop.ServerInterfaces.Query.IQuery
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IAuthentication
        and 'a :> Extensions.Configuration.IConfiguration
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IPizza
