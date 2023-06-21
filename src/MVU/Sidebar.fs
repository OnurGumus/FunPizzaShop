module FunPizzaShop.MVU.Sidebar
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = NA

type Msg = NA

type Order = NoOrder
    
let init () = Model.NA , NoOrder

let update msg model =
   model , NoOrder