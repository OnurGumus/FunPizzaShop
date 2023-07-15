module FunPizzaShop.Client.Sidebar

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
open FunPizzaShop.MVU.Sidebar
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Constants
open Thoth.Json
open FunPizzaShop.Shared.Model

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit

[<HookComponent>]
val view: host: LitElement -> model: Model -> dispatch: (Msg -> unit) -> TemplateResult

[<LitElement("fps-side-bar")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
