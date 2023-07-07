module FunPizzaShop.MVU.TrackOrder
open Elmish
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.API
open TrackOrder
open FunPizzaShop.Domain.Model
type Msg = 
   | Remote of ServerToClient.Msg

type Order = NoOrder | TrackOrder of OrderId

type Model = { Order: Pizza.Order option}


let init () = 
   {Order = None}, NoOrder
  
let update (msg:Msg) (model:Model) =
   match msg with
   | Remote (ServerToClient.Msg.OrderFound order) ->
       { Order = Some order }, NoOrder

   | Remote (ServerToClient.Msg.LocationUpdated loc) ->
         let order = { model.Order.Value with CurrentLocation = loc }
         { model with Order = Some order }, NoOrder

   | Remote (ServerToClient.Msg.ServerConnected) ->
      model, TrackOrder("Order_f829deac-4193-4bc6-9f1f-be61c6a69458" |>ShortString.TryCreate|> forceValidate |> OrderId)