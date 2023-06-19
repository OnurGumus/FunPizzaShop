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
    let host, prop = LitElement.init (fun config -> 
        let split (str: string): Topping list =
           let res = Decode.Auto.fromString<Topping list>(str, extra = extraEncoders)
           match res with
              | Ok x -> x
                | Error x -> []

        config.useShadowDom <- false
        config.props <-
        {|
            toppings = Prop.Of([], attribute="toppings", fromAttribute = split)
        |}
    )

    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    printf "%A" (prop.toppings)
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()