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
    
type Model = NoModel

type Order = NoOrder | ConnectToClient | TrackOrder of OrderId

let init () =
    NoModel, ConnectToClient

let update (msg:ServerMsg) (model:Model) =
    match msg with
    | Remote (ClientToServer.Msg.TrackOrder orderId) ->
        NoModel, TrackOrder(orderId)
    | _ ->
    NoModel,NoOrder

let execute (env:#_) (clientDispatch:Dispatch<ServerToClient.Msg>) (order:Order) dispatch =
    match order with
    | ConnectToClient -> 
        clientDispatch ServerToClient.ServerConnected
    | TrackOrder orderId ->
        printfn "TrackOrder1: %A\n" orderId
        let query = env :> IQuery
        async {
            printfn "TrackOrder2: %A\n" orderId
            let! orders = 
                query.Query<Pizza.Order>(filter = Equal("OrderId", orderId.Value.Value), take = 1)
            orders |> Seq.head |> ServerToClient.OrderFound |> clientDispatch
            printfn "TrackOrder3: %A\n" orderId
        } |> Async.StartImmediate
    | _ -> ()

let brideServer (env:#_) : HttpHandler =
    mkBridgeProgramWithOrderExecute TrackOrder.endpoint init update (execute env)
    |> Bridge.withConsoleTrace
    |> Bridge.whenDown ClientDisconnected
    |> Bridge.run Giraffe.server