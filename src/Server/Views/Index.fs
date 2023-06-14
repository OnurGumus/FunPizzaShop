module FunPizzaShop.Server.Views.Index
open Common

let view (dataLevel: int) =
    html $""" 
        <fps-pizza-menu></fps-pizza-menu>
    """