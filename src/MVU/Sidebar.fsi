module FunPizzaShop.MVU.Sidebar

open Elmish
open FunPizzaShop.Shared.Model.Pizza

type Model = { Pizzas: Pizza list }

type Msg =
    | AddPizza of Pizza
    | RemovePizza of Pizza
    | OrderReceived of Pizza list

type Order =
    | NoOrder
    | ShowCheckout of Pizza list

val init: unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
