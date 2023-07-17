module FunPizzaShop.Server.Views.TrackOrder
open Common
open FunPizzaShop.Server
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

let view  (env:_) (orderId:string) (ctx:HttpContext) (dataLevel: int) = task{
    
    return
        html $""" 
        <fps-trackorder orderId='{ orderId }'></fps-trackorder>
        """
}