module FunPizzaShop.MVU.PizzaItem
open Elmish
open FunPizzaShop.Shared.Model.Pizza

type Model = { PizzaSpecial: PizzaSpecial }

type Msg = NA

type Order = NoOrder
    
let init (pizzaSpecial:PizzaSpecial) () = 
   {PizzaSpecial = pizzaSpecial} , NoOrder

let update msg model =
   model , NoOrder