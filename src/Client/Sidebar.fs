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
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Constants

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    Hook.useEffectOnce (fun () ->
    let handleOrderedPizza (e: Event) =
        let customEvent = e :?> CustomEvent
        let pizza = customEvent.detail :?> Pizza
        dispatch (AddPizza pizza)
    document.addEventListener (Events.PizzaOrdered, handleOrderedPizza) |> ignore

    Hook.createDisposable (fun () -> document.removeEventListener (Events.PizzaOrdered, handleOrderedPizza)))

    Lit.nothing

[<LitElement("fps-side-bar")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, _ = LitElement.init (fun config -> 

        config.useShadowDom <- false
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute 
            (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()
