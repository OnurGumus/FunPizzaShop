module FunPizzaShop.MVU.TrackOrder
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = { Pizzas: Pizza list}

type Msg = 
   | AddPizza of Pizza
   | RemovePizza of Pizza
   | OrderReceived of Pizza list

type Order = NoOrder
    
let init () = {Pizzas = []} , NoOrder

let update msg model =
      model, NoOrder