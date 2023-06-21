module FunPizzaShop.Server.Views.Index

open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open Thoth.Json.Net
open Command.Serialization

val view: env: #_ -> dataLevel: int -> System.Threading.Tasks.Task<string>
