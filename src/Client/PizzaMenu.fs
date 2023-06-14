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
open Browser.Types
open Fable.Core.JS

let private hmr = HMR.createToken ()

let init () = (), Cmd.none

let update (model: unit) (msg: unit) =
    match msg with
    | _ -> model, Cmd.none

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
        Program.mkHidden (init) (update)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()