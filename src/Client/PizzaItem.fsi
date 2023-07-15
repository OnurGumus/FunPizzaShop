module FunPizzaShop.Client.PizzaItem

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
open Browser.Types
open FunPizzaShop.MVU.PizzaItem
open Thoth.Json
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.Shared.Constants

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit

[<HookComponent>]
val view: host: LitElement -> model: Model -> dispatch: 'a -> TemplateResult

[<LitElement("fps-pizza-item")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
