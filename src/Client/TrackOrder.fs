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
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop

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

[<Global>]
let L :obj  = jsNative

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    Hook.useEffectOnce (fun () -> 
        let map = L?map("map")?setView(([|51.505; -0.09|]:obj array), 13);
        L?tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {|
            maxZoom = 19;
            attribution = "&copy; <a href='http://www.openstreetmap.org/copyright'>OpenStreetMap</a>"|})?addTo(map);
        Hook.createDisposable(fun () ->  
        (bc :> IDisposable).Dispose()
        match Elmish.Bridge.Helpers.mappings.Value with
        | Some map ->  
            Elmish.Bridge.Helpers.mappings.Value <- Some (map.Remove("TrackOrder" |> Some))
        | None -> ())
    )
    let orderDetails = 
        match model.Order with
        | Some order -> 
         
            let pizzas = order.Pizzas
            [ OrderReview.pizzaList pizzas @ [OrderReview.summ pizzas] ]
        | _ -> []
    html $"""
        <div class="track-order">
        <div class="track-order-title">
            <h2>
                Order placed { if model.Order.IsSome then model.Order.Value.CreatedTime.ToString() else "" }
            </h2>
            <p class="ml-auto mb-0">
                Status: <strong> { if model.Order.IsSome  then model.Order.Value.DeliveryStatus.ToString()  else ""}</strong>
            </p>
        </div>
        <div class="track-order-body">
            <div class="track-order-details">
                { orderDetails}
            </div>
            <div class="track-order-map">
                <div id="map" class="map">

                </div>
            </div>
        </div>
    </div>
       
    """
    

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
