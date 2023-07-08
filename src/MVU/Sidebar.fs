module FunPizzaShop.MVU.Sidebar
open Elmish
open FunPizzaShop.Shared.Model.Pizza

type Model = { Pizzas: Pizza list}

type Msg = 
   | AddPizza of Pizza
   | RemovePizza of Pizza
   | OrderReceived of Pizza list

type Order = NoOrder | ShowCheckout of Pizza list
    
let init () = {Pizzas = []} , NoOrder

let update msg model =
      match msg with
      | AddPizza pizza -> 
         ({model with Pizzas = pizza :: model.Pizzas}:Model), NoOrder
         
      | RemovePizza pizza -> 
         {model with Pizzas = List.filter (fun p -> p <> pizza) model.Pizzas}, NoOrder
      | OrderReceived pizzas -> 
         {model with Pizzas = pizzas}, ShowCheckout pizzas