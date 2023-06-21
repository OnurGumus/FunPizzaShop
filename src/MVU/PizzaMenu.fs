module FunPizzaShop.MVU.PizzaMenu
open Elmish
open FunPizzaShop.Domain.Model.Pizza


type Model = { Pizza : Pizza option; Toppings : Topping list }

type Msg = 
   | PizzaConfirmed 
   | PizzaCancelled 
   | ToppingRemoved of Topping 
   | ToppingAdded of int
   | PizzaSelected of Pizza
   | SizeChanged of int

type Order = NoOrder
    
let init (toppings: Topping list) () = {Pizza = Option.None; Toppings = toppings} , NoOrder

let update msg model =
   match msg with
   | PizzaConfirmed -> {model with Pizza = Option.None} , NoOrder
   | PizzaCancelled -> {model with Pizza = Option.None} , NoOrder
   | SizeChanged size -> 
      match model.Pizza with
      | Some pizza -> 
         let newPizza = { pizza with Size = size }
         {model with Pizza = Some newPizza} , NoOrder
      | None -> model , NoOrder
   | ToppingAdded index ->
      match model.Pizza with
      | Some pizza -> 
         let topping = List.item index model.Toppings
         let newPizza = { pizza with Toppings = topping :: pizza.Toppings}
         {model with Pizza = Some newPizza} , NoOrder
      | None -> model , NoOrder
   | ToppingRemoved topping -> 
      match model.Pizza with
      | Some pizza -> 
         let newPizza = pizza// Pizza.RemoveTopping topping pizza
         {model with Pizza = Some newPizza} , NoOrder
      | None -> model , NoOrder
   | PizzaSelected pizza -> {model with Pizza = Some pizza} , NoOrder
   