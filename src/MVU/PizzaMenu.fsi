module FunPizzaShop.MVU.PizzaMenu

open Elmish
open FunPizzaShop.Shared.Model.Pizza

type Model =
    { Pizza: Pizza option
      Toppings: Topping list }

type Msg =
    | PizzaConfirmed
    | PizzaCancelled
    | ToppingRemoved of Topping
    | ToppingAdded of int
    | PizzaSelected of Pizza
    | SizeChanged of int

type Order = NoOrder
val init: toppings: Topping list -> unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
