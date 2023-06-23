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
open Thoth.Json
open FunPizzaShop.Domain.Model

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()
    | Order.ShowCheckout pizzas ->
        let pizzaString = Encode.Auto.toString(pizzas,extra = extraEncoders)
        CustomNavigation.newUrl (CustomNavigation.toPage CustomNavigation.Checkout) pizzaString |> ignore

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    Hook.useEffectOnce (fun () ->
    let handleOrderedPizza (e: Event) =
        let customEvent = e :?> CustomEvent
        let pizza = customEvent.detail :?> Pizza
        dispatch (AddPizza pizza)
    document.addEventListener (Events.PizzaOrdered, handleOrderedPizza) |> ignore

    Hook.createDisposable (fun () -> document.removeEventListener (Events.PizzaOrdered, handleOrderedPizza)))
    if model.Pizzas.Length = 0 then
        html $"""
        <div class= "empty-cart">
            Choose a pizza <br/>
            to get started
        </div>
        """
    else
        let cartItem index (pizza: Pizza) =
            let toppings  =
                pizza.Toppings 
                    |> Lit.mapiUnique 
                        (fun (t:Topping )-> t.Name.Value) 
                        (fun index (t: Topping) ->html $"""<li>+{t.Name.Value}</li>""")
            html $"""
                <div class="cart-item">
                    <button class="delete-topping" @click={Ev(fun _ -> dispatch (RemovePizza pizza))}>x</button>
                    <div class="title">
                        {pizza.Size.ToString()} { pizza.Special.Name.Value }
                    </div>
                    <ul>
                        {toppings}
                    </ul>
                    <div class="item-price">
                        { pizza.FormattedTotalPrice }
                    </div>
                </div>
            """
        let cartItems = model.Pizzas |> Lit.mapiUnique (fun (p:Pizza) -> p.Id.ToString()) cartItem

        html $"""
            <div class = "order-contents">
                <h2> Your order </h2>
                {cartItems}
            </div>
            <div class="order-total">
                Total:
                <span class= "total-price">
                        { model.Pizzas |> List.sumBy (fun p -> Math.Round(p.TotalPrice.Value,2)) |> string }
                </span>
                <button @click={Ev(fun _ -> dispatch (OrderReceived model.Pizzas))} 
                    class="btn btn-warning">Order ></button>
            </div>
         """
                   
                

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
