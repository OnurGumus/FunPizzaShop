module FunPizzaShop.Client.SignIn

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
open FunPizzaShop.MVU
open SignIn
open FunPizzaShop.Shared.Model
open Pizza
open FunPizzaShop.Shared.Constants
open Authentication

module Server =
    open Fable.Remoting.Client
    open FunPizzaShop.Shared

    val api: queryString: string option -> API.Authentication

val execute: host: LitElement -> order: Order -> dispatch: (Msg -> unit) -> unit

[<HookComponent>]
val view: host: LitElement -> model: Model -> dispatch: (Msg -> unit) -> TemplateResult

[<LitElement("fps-signin")>]
val LitElement: unit -> TemplateResult

val register: unit -> unit
