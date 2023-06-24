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
   
    let pizzaTemplate (pizza: Pizza) =
        let toppingItem (topping: Topping) =
            html $"""<li>+ {topping.Name.Value}</li>"""

        let toppingItems =
            pizza.Toppings
            |> List.map toppingItem

        html $"""
            <p>
                <strong>
                    { pizza.Size}"
                    { pizza.Special.Name.Value }
                    (£{ pizza.Special.FormattedBasePrice })
                </strong>
            </p>

            <ul>
                {toppingItems}
            </ul>

    """
    let sum = 
        pizzas 
        |> List.sumBy (fun (p:Pizza) -> p.TotalPrice.Value)

    let summ  = 
        html $"""
            <p>
                <strong>
                    Total price:
                    £{ sum.ToString("0.00") }
                </strong>
            </p>
        """
    let pizzaList = 
        pizzas 
        |> List.map (pizzaTemplate)

    let formFieldItem (label:string)=
        html $"""
            <div class="form-field">
            <label>{label}:</label>
                <div>
                    <input class="$ValidClass" type="text"  />
                </div>
            </div>
        """
    let formItems =
                 [
                    formFieldItem  "Name"
                    formFieldItem  "City"
                    formFieldItem  "Region" 
                    formFieldItem  "Postal Code"
                    formFieldItem  "Address Line 1"
                    formFieldItem  "Address Line 2"
                ]
    html $"""
        <div class='main'>
        <div class="checkout-cols">
            <div class="checkout-order-details">
                <h4>Review order</h4>
                { pizzaList }
                { summ }
            </div>

            <div class="checkout-delivery-address">
                <h4>Deliver to...</h4>
                <form>
                { formItems}
                </form>
            </div>
        </div>
        <button class="checkout-button btn btn-warning">
            Place order
        </button>
        </div>
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
