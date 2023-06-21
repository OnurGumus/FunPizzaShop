module FunPizzaShop.MVU.Sidebar
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = { Pizzas: Pizza list}

type Msg = 
   | AddPizza of Pizza
   | RemovePizza of Pizza

type Order = NoOrder
    
let init () = {Pizzas = []} , NoOrder

let update msg model =
      match msg with
      | AddPizza pizza -> 
         {model with Pizzas = pizza :: model.Pizzas}, NoOrder
         
      | RemovePizza pizza -> 
         {model with Pizzas = List.filter (fun p -> p <> pizza) model.Pizzas}, NoOrder