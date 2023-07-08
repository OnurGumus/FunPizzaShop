module FunPizzaShop.Server.Query
open FunPizzaShop.Shared.Model
open Akka.Streams.Dsl
open Akka.Streams
open FunPizzaShop.Query.Projection

[<Interface>]
type IQuery =
    abstract Query<'t> : ?filter:Predicate * ?orderby:string * ?orderbydesc:string * ?thenby:string  * ?thenbydesc:string * ?take:int * ?skip:int -> list<'t> Async
    abstract Subscribe:(DataEvent -> unit)->IKillSwitch