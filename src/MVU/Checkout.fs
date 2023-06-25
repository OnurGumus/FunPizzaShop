module FunPizzaShop.MVU.Checkout
open Elmish
open FunPizzaShop.Domain.Model.Pizza

type Model = { Pizzas: Pizza list; IsLoggedIn : bool; PendingOrder : string option}

type Msg = SetPizzas of Pizza list | OrderPlaced of string | SetLoginStatus of bool


type Order =
      | NoOrder 
      | GetPizzas 
      | PlaceOrder of string 
      | SubscribeToLogin 
      | OrderList of Order list
      | RequestLogin
    
let init () =  
      { Pizzas = []; IsLoggedIn = false; PendingOrder = None } , 
      [GetPizzas;SubscribeToLogin] |> OrderList

let update msg model =
      match msg with
      | SetPizzas pizzas -> {model with Pizzas = pizzas}, NoOrder
      | OrderPlaced orderId -> 
            if model.IsLoggedIn |> not then
                  {model with PendingOrder = Some orderId}, RequestLogin
            else
                  model, PlaceOrder orderId
                  
      | SetLoginStatus isLoggedIn -> 
            if isLoggedIn then
                  match model.PendingOrder with
                  | Some orderId -> {model with PendingOrder = None}, PlaceOrder orderId
                  | None -> model, NoOrder
            else
            {model with IsLoggedIn = isLoggedIn}, NoOrder