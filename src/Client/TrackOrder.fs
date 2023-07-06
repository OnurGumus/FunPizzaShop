module FunPizzaShop.Client.TrackOrder

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
open FunPizzaShop.MVU.TrackOrder
open Thoth.Json
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open FunPizzaShop.Domain.Constants
open CustomNavigation
open FunPizzaShop.Domain

let private hmr = HMR.createToken ()

module Server =
    open Fable.Remoting.Client
    open FunPizzaShop.Domain
    open FunPizzaShop.Domain.Model.Pizza
    let api: API.Order =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder API.Route.builder
        |> Remoting.buildProxy<API.Order>

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()
   

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    html $"""
        <div class='main'>
            TrackOrder
        </div>
    """

[<LitElement("fps-trackorder")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, _ = LitElement.init (fun config -> 
        config.useShadowDom <- false
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()
