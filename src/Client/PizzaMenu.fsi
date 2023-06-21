module FunPizzaShop.Client.PizzaMenu

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
open FunPizzaShop.MVU.PizzaMenu
open Thoth.Json
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open FunPizzaShop.Domain
open FunPizzaShop.Domain.Constants

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit

[<HookComponent>]
val view: host: 'a -> model: Model -> dispatch: (Msg -> unit) -> TemplateResult

[<LitElement("fps-pizza-menu")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
