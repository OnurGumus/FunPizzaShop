module FunPizzaShop.Server.Views.MyOrders

open Common
open FunPizzaShop.Server
open FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

val view: env: #IQuery -> ctx: HttpContext -> dataLevel: int -> System.Threading.Tasks.Task<string>
