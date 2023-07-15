module FunPizzaShop.Client.LoginStore

open ElmishOrder
open FunPizzaShop.MVU.LoginStore

val execute: order: Order -> dispatch: 'a -> unit
