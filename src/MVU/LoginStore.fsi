module FunPizzaShop.MVU.LoginStore

open ElmishStore
open FunPizzaShop.Shared.Model.Authentication

type Model = { UserId: UserId option }

type Msg =
    | LoggedIn of UserId
    | LoggedOut

[<RequireQualifiedAccessAttribute>]
type Order = | NoOrder

val init: unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
val dispose: 'a -> unit
