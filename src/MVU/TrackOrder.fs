module FunPizzaShop.MVU.TrackOrder
open Elmish
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.API
type Model = { Pizzas: Pizza list}

type Msg = 
   | Remote of ServerToClient.Msg

type Order = NoOrder
    
let init () = {Pizzas = []} , NoOrder

let update msg model =
      model, NoOrder