module FunPizzaShop.Server.Views.MyOrders
open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

let view (env:#_) (ctx:HttpContext)  (dataLevel: int) = task{
    let query = env :> IQuery
    let! orders = query.Query<Order> (filter = Equal ("UserId", ctx.User.Identity.Name), take = 20, orderbydesc = "CreatedTime")
    let li = 
        orders |> List.map(fun order ->
            html $"""
            <div class="list-group-item">
                <div class="col">
                    <h5> { order.CreatedTime } </h5>
                    Items:
                    <strong> { order.Pizzas.Length } </strong>;
                    Total price:
                    <strong>£{ order.FormattedTotalPrice }</strong>
                </div>
                <div class="col">
                    Status: <strong>{ order.DeliveryStatus }</strong>
                </div>
                <div class="col flex-grow-0">
                    <a href='trackOrder/{ order.OrderId.Value }' class="btn btn-success">
                        Track &gt;
                    </a>
                </div>
            </div>
            """)
        |> String.concat "\r\n"
    return
        html $""" 
        <div class=main>
        <div class="list-group orders-list">
            {li}
        </div>
        </div>
        """
}