module FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model
open Akka.Streams.Dsl
open Akka.Streams
open FunPizzaShop.Shared.Model.Pizza

type OrderEvent = 
    | OrderPlaced of Order 
    | DeliveryStatusSet of OrderId * DeliveryStatus
    | LocationUpdated of OrderId * LatLong

type DataEvent = OrderEvent of OrderEvent

[<Interface>]
type IQuery =
    abstract Query<'t> : ?filter:Predicate * ?orderby:string * ?orderbydesc:string * ?thenby:string  * ?thenbydesc:string * ?take:int * ?skip:int -> list<'t> Async
    abstract Subscribe:(DataEvent -> unit)->IKillSwitch