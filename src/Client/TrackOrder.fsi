module FunPizzaShop.Client.TrackOrder

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open System
open Elmish.Debug
open ElmishOrder
open FunPizzaShop.MVU
open FunPizzaShop.MVU.TrackOrder
open Elmish.Bridge
open FunPizzaShop.Shared.API
open TrackOrder
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit
val mapClientMsg: msg: ServerToClient.Msg -> Msg
val bc: BridgeConfig<ServerToClient.Msg, Msg>

[<Global>]
val L: obj

[<HookComponent>]
val view: host: LitElement -> model: Model -> dispatch: 'a -> TemplateResult

[<LitElement("fps-trackorder")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
