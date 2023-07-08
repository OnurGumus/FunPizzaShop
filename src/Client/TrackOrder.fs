module FunPizzaShop.Client.TrackOrder

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open System
open Elmish.Debug
open ElmishOrder
open FunPizzaShop.MVU
open FunPizzaShop.MVU.TrackOrder
open Elmish.Bridge
open FunPizzaShop.Domain.API
open TrackOrder

#if DEBUG
let private hmr = HMR.createToken ()
#endif

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()
    | Order.TrackOrder orderId ->
        Bridge.NamedSend("TrackOrder", (ClientToServer.Msg.TrackOrder orderId))
    
let mapClientMsg msg =
    match msg with
    | _ -> Remote msg

let bc = 
    Bridge.endpoint 
        endpoint 
        |> Bridge.withUrlMode UrlMode.Replace 
        |> Bridge.withMapping mapClientMsg 
        |> Bridge.withName "TrackOrder"

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    Hook.useEffectOnce (fun () -> 
        Hook.createDisposable(fun () ->  
        (bc :> IDisposable).Dispose()
        match Elmish.Bridge.Helpers.mappings.Value with
        | Some map ->  
            Elmish.Bridge.Helpers.mappings.Value <- Some (map.Remove("TrackOrder" |> Some))
        | None -> ())
    )
    match model.Order with
    | Some order ->
        let pizzas = order.Pizzas
        html $"""
        <div class="track-order">
        <div class="track-order-title">
            <h2>
                Order placed { model.Order.Value.CreatedTime }
            </h2>
            <p class="ml-auto mb-0">
                Status: <strong> { order.DeliveryStatus }</strong>
            </p>
        </div>
        <div class="track-order-body">
            <div class="track-order-details">
                { OrderReview.pizzaList pizzas }
                { OrderReview.summ pizzas }
            </div>
            <div class="track-order-map">
                <!-- ${Map} -->
            </div>
        </div>
    </div>
       
    """
    | _ -> Lit.nothing

[<LitElement("fps-trackorder")>]
let LitElement () =

#if DEBUG
    Hook.useHmr (hmr)
#endif

    let host, _ = LitElement.init (fun config -> 
        config.useShadowDom <- false
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
        |> Program.withBridgeConfig bc
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()
