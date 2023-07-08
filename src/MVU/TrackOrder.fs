module FunPizzaShop.MVU.TrackOrder
open Elmish
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.API
open TrackOrder
open FunPizzaShop.Shared.Model
type Msg = 
   | Remote of ServerToClient.Msg

type Order = NoOrder | TrackOrder of OrderId

type Model = { Order: Pizza.Order option; OrderId : OrderId }


let init (orderId:string) () =
   let orderId = orderId |> ShortString.TryCreate |> forceValidate |> OrderId
   {Order = None; OrderId = orderId}, NoOrder
  
let update (msg:Msg) (model:Model) =
   match msg with
   | Remote (ServerToClient.Msg.OrderFound order) ->
       { model with  Order = Some order }, NoOrder

   | Remote (ServerToClient.Msg.LocationUpdated(_, loc)) ->
         let order = { model.Order.Value with CurrentLocation = loc }
         { model with Order = Some order }, NoOrder
   | Remote (ServerToClient.Msg.DeliveryStatusSet(_, status)) ->
         let order = { model.Order.Value with DeliveryStatus = status }
         { model with Order = Some order }, NoOrder

   | Remote (ServerToClient.Msg.ServerConnected) ->
      model, TrackOrder(model.OrderId)