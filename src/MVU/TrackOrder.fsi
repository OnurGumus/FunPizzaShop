module FunPizzaShop.MVU.TrackOrder

open Elmish
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.API
open TrackOrder
open FunPizzaShop.Shared.Model

type Msg = Remote of ServerToClient.Msg

type Order =
    | NoOrder
    | TrackOrder of OrderId

type Model =
    { Order: Pizza.Order option
      OrderId: OrderId }

val init: orderId: string -> unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
