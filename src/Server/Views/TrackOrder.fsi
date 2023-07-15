module FunPizzaShop.Server.Views.TrackOrder

open Common
open FunPizzaShop.Server
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

val view: env: 'a -> orderId: string -> ctx: HttpContext -> dataLevel: int -> System.Threading.Tasks.Task<string>
