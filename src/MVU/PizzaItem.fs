module FunPizzaShop.MVU.PizzaItem
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = { PizzaSpecial: PizzaSpecial }

type Msg = NA

type Order = None
    
let init (pizzaSpecial:PizzaSpecial) () = 
   {PizzaSpecial = pizzaSpecial} , None

let update msg model =
   model , None