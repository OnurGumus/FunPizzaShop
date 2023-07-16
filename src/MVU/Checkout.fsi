module FunPizzaShop.MVU.Checkout

open Elmish
open FunPizzaShop.Shared
open Model
open Pizza
open Authentication
open System

type OrderDetails =
    { Address: Address; Pizzas: Pizza list }

type Model =
    { Pizzas: Pizza list
      UserId: UserId option
      PendingOrder: OrderDetails option }

type Msg =
    | SetPizzas of Pizza list
    | OrderCheckedOut of OrderDetails
    | SetLoginStatus of UserId option
    | OrderPlaced

type Order =
    | NoOrder
    | GetPizzas
    | PlaceOrder of Pizza.Order
    | SubscribeToLogin
    | OrderList of Order list
    | RequestLogin

val init: unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
