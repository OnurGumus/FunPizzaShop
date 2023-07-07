module FunPizzaShop.Server.Views.TrackOrder
open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open Thoth.Json.Net
open Command.Serialization
open Microsoft.AspNetCore.Http

let view  (env:#_) (ctx:HttpContext) (dataLevel: int) = task{
    
    return
        html $""" 
        <fps-trackorder></fps-trackorder>
        """
}