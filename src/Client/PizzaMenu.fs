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
open Fable.Core.JS
open FunPizzaShop.MVU.PizzaMenu

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.None -> ()


let view host model dispatch =
    html
        $"""
        <h2>
            Pizza Menu
        </h2>
        """

[<LitElement("fps-pizza-menu")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, _ = LitElement.init (fun config -> config.useShadowDom <- false)

    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()