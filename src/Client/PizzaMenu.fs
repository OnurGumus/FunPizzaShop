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

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()

[<HookComponent>]
let view host model dispatch =
    Hook.useEffectOnce (fun () -> 
        let handleSelectedPizza (e:Event) =
            let customEvent = e :?> CustomEvent
            let pizzaSpecial = customEvent.detail :?> Pizza.PizzaSpecial
            let pizza = Pizza.CreatePizzaFromSpecial pizzaSpecial
            dispatch (PizzaSelected pizza)

        host?addEventListener(Events.PizzaSelected, handleSelectedPizza) |> ignore

        Hook.createDisposable (fun () ->host?removeEventListener (Events.PizzaSelected, handleSelectedPizza))
    )
    let dialog (pizza:Pizza.Pizza) = 
        html $"""
        <div class="dialog-container">
            <div class="dialog">
                <div class="dialog-title">
                    <h2> { pizza.Special.Name } </h2>
                    { pizza.Special.Description }
                </div>
                <form class="dialog-body">
                    ToppingCombo
                    ToppingItems
                    <div>
                        <label>Size:</label>
                        <input type="range" .value={ pizza.Size } min={ Pizza.MinimumSize }
                            max= { Pizza.MaximumSize } step="1" @input={ Ev (fun e -> dispatch (SizeChanged e.target?value) )}  />
                        <span class="size-label">
                            { pizza.Size }" (£{ pizza.FormattedTotalPrice })
                        </span>
                    </div>
                </form>
                <div class="dialog-buttons">
                    <button id="cancelButton" class="btn btn-secondary mr-auto" @click={Ev(fun _ -> dispatch PizzaCancelled)}>Cancel</button>
                    <span class="mr-center">
                        Price: <span class="price">{ pizza.FormattedTotalPrice }</span>
                    </span>
                    <button id="confirmButton" class="btn btn-success ml-auto" @click={Ev(fun _ -> dispatch PizzaConfirmed)}>Order ></button>
                </div>
            </div>
        </div>
        """
    match model.Pizza with
    | Some pizza -> dialog pizza
    | None ->
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
    printfn "%A" prop.toppings.Value
    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()