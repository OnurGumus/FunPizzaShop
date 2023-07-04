module FunPizzaShop.Server.Views.MyOrders
open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

let view (ctx:HttpContext) (env:#_) (dataLevel: int) = task{
    let query = env :> IQuery
    let! orders = query.Query<Order> (filter = Equal ("UserId", ctx.User.Identity.Name), take = 20, orderbydesc = "CreatedTime")
    let li = 
        orders |> List.map(fun order ->
            html $"""
                <li>
                  { order.CreatedTime }  { order.DeliveryStatus }
                </li>
            """)
        |> String.concat "\r\n"
    return
        html $""" 
            <ul class="pizza-cards">
                    {li}
            </ul>
        """
}