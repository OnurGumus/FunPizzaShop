module FunPizzaShop.MVU.PizzaItem

open Elmish
open FunPizzaShop.Shared.Model.Pizza

type Model = { PizzaSpecial: PizzaSpecial }
type Msg = NA
type Order = NoOrder
val init: pizzaSpecial: PizzaSpecial -> unit -> Model * Order
val update: msg: 'a -> model: 'b -> 'b * Order
