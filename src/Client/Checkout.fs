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
open FunPizzaShop.MVU
open FunPizzaShop.MVU.Checkout
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
    | Order.GetPizzas ->
        let pizzaString:string =  history.state |> unbox<string>
        let pizzas = Decode.Auto.fromString<Pizza list>(pizzaString,extra = extraEncoders)
        match pizzas with
        | Ok pizzas ->
            dispatch (SetPizzas pizzas) 
        | Error err -> console.error err
    | Order.PlaceOrder order ->
        async {
            do! Server.api.OrderPizza order
            dispatch  OrderPlaced
            history.replaceState (null, "", sprintf "order/%s" order.OrderId.Value.Value)
            let ev = CustomEvent.Create(NavigatedEvent)
            window.dispatchEvent ev |> ignore
        }
        |> Async.StartImmediate
      
    | Order.RequestLogin ->
        host.dispatchCustomEvent (Constants.Events.RequestLogin, null,true,true,true)
        
    | Order.SubscribeToLogin ->
        (LoginStore.store.Subscribe (fun (model:LoginStore.Model) -> dispatch (SetLoginStatus model.UserId))  )
        |> ignore
    | Order.OrderList orders ->
        orders
        |> List.iter (fun order -> execute host order dispatch)

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    let pizzas = model.Pizzas
   
    

    let formFieldItem (label:string)=
        html $"""
            <div class="form-field">
            <label>{label}:</label>
                <div>
                    <input required name='{ label.Replace(" ","")  }' type="text"  />
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
    let onSubmit (e:Browser.Types.Event) =
        e.preventDefault()
        let form = e.target :?> HTMLFormElement
        let name = form?Name?value |> ShortString.TryCreate |> forceValidate
        let city = form?City?value |> ShortString.TryCreate |> forceValidate
        let region = form?Region?value |> ShortString.TryCreate |> forceValidate
        let postalCode = form?PostalCode?value |> ShortString.TryCreate |> forceValidate
        let addressLine1 = form?AddressLine1?value |> ShortString.TryCreate |> forceValidate
        let addressLine2 = form?AddressLine2?value |> ShortString.TryCreate |> forceValidate
        let address:Address =  {
            Name = name
            City = city
            Region = region
            PostalCode = postalCode
            Line1 = addressLine1
            Line2 = addressLine2
        }
        let order:OrderDetails = {
            Pizzas = pizzas
            Address = address
        }
        dispatch (OrderCheckedOut order)
    html $"""
        <div class='main'>
        <div class="checkout-cols">
            <div class="checkout-order-details">
                <h4>Review order</h4>
                { OrderReview.pizzaList pizzas }
                { OrderReview.summ pizzas }
            </div>

            <div class="checkout-delivery-address">
                <h4>Deliver to...</h4>
                <form id="checkout" @submit={Ev(fun e -> onSubmit(e))}>
                { formItems}
                </form>
            </div>
        </div>
        <button class="checkout-button btn btn-warning" form=checkout type="submit" >
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
