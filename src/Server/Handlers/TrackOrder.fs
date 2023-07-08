module FunPizzaShop.Server.Handlers.TrackOrder
open FunPizzaShop.Server.Views
open Giraffe
open Elmish.Bridge
open FunPizzaShop.Domain.API
open Elmish
open TrackOrder
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model
open FunPizzaShop.Domain.Model.Pizza
open Akka.Streams
open FunPizzaShop.Query.Projection

let mkBridgeProgramWithOrderExecute endpoint
    (init: 'arg -> 'model * 'order)
    (update:  'msg -> 'model -> 'model * 'order)
    (execute:  Dispatch<'serverToClient> -> 'order  -> Dispatch<'msg> -> unit) =
    let convert (dispatch:Dispatch<'serverToClient>) ((model:'model), order) = 
        model, order |> (execute dispatch) |> Cmd.ofEffect
    Bridge.mkServer endpoint
        (fun dispatch arg -> init arg |> (convert dispatch))
        (fun dispatch msg model -> (update msg model) |> (convert dispatch))

type ServerMsg =
        | Remote of ClientToServer.Msg
        | ClientDisconnected
        | SetKillSwitch of IKillSwitch
    
type Model = { KillSwitch: IKillSwitch option}

type Order = NoOrder | ConnectToClient | TrackOrder of OrderId

let init () =
    {KillSwitch = None }, ConnectToClient

let update (msg:ServerMsg) (model:Model) =
    match msg with
    | Remote (ClientToServer.Msg.TrackOrder orderId) ->
        model, TrackOrder(orderId)
    | SetKillSwitch ks ->
        { model with KillSwitch = Some ks }, NoOrder
    | ClientDisconnected ->
        if model.KillSwitch.IsSome then
            model.KillSwitch.Value.Shutdown()
        { model with KillSwitch = None }, NoOrder


let execute (env:#_) (clientDispatch:Dispatch<ServerToClient.Msg>) (order:Order) dispatch =
    match order with
    | ConnectToClient -> 
        clientDispatch ServerToClient.ServerConnected
    | TrackOrder orderId ->
        let query = env :> IQuery
        async {
            let! orders = 
                query.Query<Pizza.Order>(filter = Equal("OrderId", orderId.Value.Value), take = 1)
            orders |> Seq.head |> ServerToClient.OrderFound |> clientDispatch

            let ks =
                query.Subscribe(fun event -> 
                    match event with
                    | OrderEvent (LocationUpdated(orderId, loc)) when orderId = orderId ->
                         ServerToClient.LocationUpdated (orderId,loc) |> clientDispatch
                    | OrderEvent (DeliveryStatusSet(orderId, status)) when orderId = orderId ->
                        ServerToClient.DeliveryStatusSet(orderId,status) |> clientDispatch
                    | _ -> ()
                )
            SetKillSwitch ks |> dispatch
        } |> Async.StartImmediate
    | NoOrder -> ()

let brideServer (env:#_) : HttpHandler =
    mkBridgeProgramWithOrderExecute TrackOrder.endpoint init update (execute env)
    |> Bridge.withConsoleTrace
    |> Bridge.whenDown ClientDisconnected
    |> Bridge.run Giraffe.server