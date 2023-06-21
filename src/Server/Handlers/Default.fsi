module BestFitBox.Server.Handlers.Default

open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views

val webApp:
    env: #_ ->
    layout: (HttpContext -> (int -> Task<string>) -> string Task) ->
        (HttpFunc -> HttpContext -> HttpFuncResult)

val webAppWrapper:
    env: #_ ->
    layout: (HttpContext -> (int -> Task<string>) -> string Task) ->
        (HttpFunc -> HttpContext -> Task<HttpContext option>)
