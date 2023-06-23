module FunPizzaShop.MVU.Checkout
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = { Pizzas: Pizza list}

type Msg = SetPizzas of Pizza list

type Order = NoOrder | GetPizzas
    
let init () =  { Pizzas = []} , GetPizzas

let update msg model =
      match msg with
      | SetPizzas pizzas -> {model with Pizzas = pizzas}, NoOrder