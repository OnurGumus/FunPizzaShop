module FunPizzaShop.Server.Views.Index
open Common
open FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

let view (env:_) (ctx:HttpContext) (dataLevel: int) = task{
    let query = env :> IQuery
    let! pizzas = query.Query<PizzaSpecial> (filter = Greater("BasePrice",1m)) 
    let! toppings = query.Query<Topping> ()
    let serializedToppings = Encode.Auto.toString (toppings, extra = extraThoth)
    let li = 
        pizzas 
        |> List.map (fun pizza -> 
        let serializedSpecial = 
            Encode.Auto.toString(pizza, extra = extraThoth)
        let serializedSpecial = System.Net.WebUtility.HtmlEncode serializedSpecial
        html $"""
            <li>
                <fps-pizza-item special='{serializedSpecial}'>
                    <div class="pizza-info" style="background-image: url('/assets/{pizza.ImageUrl}')">
                        <span class=title>{pizza.Name}</span>
                        {pizza.Description}
                        <span class=price>{pizza.FormattedBasePrice}</span>
                    </div>
                </fps-pizza-item>
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
            <div class="sidebar">
                <fps-side-bar></fps-side-bar>
            </div>
        """
}