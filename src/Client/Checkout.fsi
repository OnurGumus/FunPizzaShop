module FunPizzaShop.Client.Checkout

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core
open System
open Browser
open Elmish.Debug
open FsToolkit.ErrorHandling
open ElmishOrder
open FunPizzaShop.MVU
open FunPizzaShop.MVU.Checkout
open Thoth.Json
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.Shared.Constants
open CustomNavigation
open FunPizzaShop.Shared

module Server =
    open Fable.Remoting.Client
    val api: API.Order

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit

[<HookComponent>]
val view: host: LitElement -> model: Model -> dispatch: (Msg -> unit) -> TemplateResult

[<LitElement("fps-checkout")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
