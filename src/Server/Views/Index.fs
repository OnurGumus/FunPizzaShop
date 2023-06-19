module FunPizzaShop.Server.Views.Index
open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open Thoth.Json.Net
open Command.Serialization

let view (env:#_) (dataLevel: int) = task{
    let query = env :> IQuery
    let! pizzas = query.Query<PizzaSpecial> () 
    let! toppings = query.Query<Topping> ()
    let serializedToppings = Encode.Auto.toString (toppings, extra = extraThoth)
    let li = 
        pizzas 
        |> List.map (fun pizza -> 
        html $"""
            <li style="background-image: url('/assets/{pizza.ImageUrl}')">
            <div class="pizza-info">
                <span class=title>{pizza.Name}</span>
                {pizza.Description}
                <span class=price>{pizza.FormattedBasePrice}</span>
            </div>
    </li>
        """)
        |> String.concat "\r\n"
    return
        html $""" 
            <fps-pizza-menu toppings='{serializedToppings}'>
                <ul class="pizza-cards">
                {li}
                </ul>
            </fps-pizza-menu>
        """
}