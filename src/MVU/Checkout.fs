module FunPizzaShop.MVU.Checkout
open Elmish
open FunPizzaShop.Domain
open Model
open Pizza
open Authentication
open System

type OrderDetails = { Address : Address; Pizzas : Pizza list }

type Model = { Pizzas: Pizza list; UserId : UserId option; PendingOrder : OrderDetails option}

type Msg = SetPizzas of Pizza list | OrderPlaced of OrderDetails | SetLoginStatus of UserId option


type Order =
      | NoOrder 
      | GetPizzas 
      | PlaceOrder of Pizza.Order 
      | SubscribeToLogin 
      | OrderList of Order list
      | RequestLogin
    
let init () =  
      { Pizzas = []; UserId = None; PendingOrder = None } , 
      [GetPizzas;SubscribeToLogin] |> OrderList

let rec update msg model =
      match msg with
      | SetPizzas pizzas -> ({model with Pizzas = pizzas}: Model), NoOrder
      | OrderPlaced orderDetails -> 
            if model.UserId.IsNone then
                  {model with PendingOrder = Some orderDetails}, RequestLogin
            else
                  let order :FunPizzaShop.Domain.Model.Pizza.Order = {
                        DeliveryAddress = orderDetails.Address
                        Pizzas = orderDetails.Pizzas
                        UserId = model.UserId.Value
                        OrderId = Guid.NewGuid().ToString() |> ShortString.TryCreate |> forceValidate |> OrderId
                        CreatedTime = DateTime.UtcNow
                        DeliveryLocation =  {Latitude = 0.0; Longitude = 0.0 }
                        Version = Model.Version 0L
                        CurrentLocation = {Latitude = 0.0; Longitude = 0.0 }
                        DeliveryStatus=DeliveryStatus.NotDelivered

                  }
                  model, Order.PlaceOrder order
                  
      | SetLoginStatus userId -> 
            if userId.IsSome then
                  match model.PendingOrder with
                  | Some orderDetails -> 
                        let model = {model with PendingOrder = None}
                        update (OrderPlaced orderDetails) model
                  | None -> model, NoOrder
            else
            {model with UserId = userId}, NoOrder