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
open Browser.Types
open FunPizzaShop.MVU.Checkout
open Thoth.Json
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model
open FunPizzaShop.Domain.Constants

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()
    | Order.GetPizzas ->
        let pizzaString:string =  history.state |> unbox<string>
        let pizzas = Decode.Auto.fromString<Pizza list>(pizzaString,extra = extraEncoders)
        match pizzas with
        | Ok pizzas ->
            dispatch (SetPizzas pizzas)
        | Error err -> console.error err

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    let pizzas = model.Pizzas
    let pizzaLIs = pizzas |> List.map (fun pizza -> html $"""<li>{pizza.Size.ToString()} { pizza.Special.Name.Value }</li>""")
    html $"""
        <h1>Checkout</h1>
        {pizzaLIs}
    """

[<LitElement("fps-checkout")>]
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
