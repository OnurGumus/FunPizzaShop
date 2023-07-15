module FunPizzaShop.Server.Handlers.TrackOrder

open FunPizzaShop.Server.Views
open Giraffe
open Elmish.Bridge
open FunPizzaShop.Shared.API
open Elmish
open TrackOrder
open FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model
open FunPizzaShop.Shared.Model.Pizza
open Akka.Streams
open FunPizzaShop.Query.Projection

val mkBridgeProgramWithOrderExecute:
    endpoint: string ->
    init: ('arg -> 'model * 'order) ->
    update: ('msg -> 'model -> 'model * 'order) ->
    execute: (Dispatch<'serverToClient> -> 'order -> Dispatch<'msg> -> unit) ->
        BridgeServer<'arg, 'model, 'msg, 'serverToClient, 'a>

type ServerMsg =
    | Remote of ClientToServer.Msg
    | ClientDisconnected
    | SetKillSwitch of IKillSwitch

type Model = { KillSwitch: IKillSwitch option }

type Order =
    | NoOrder
    | ConnectToClient
    | TrackOrder of OrderId

val init: unit -> Model * Order
val update: msg: ServerMsg -> model: Model -> Model * Order
val retry: f: (unit -> Async<'a option>) -> Async<'a option>

val execute:
    env: #IQuery ->
    clientDispatch: Dispatch<ServerToClient.Msg> ->
    order: Order ->
    dispatch: (ServerMsg -> unit) ->
        unit

val brideServer: env: #IQuery -> HttpHandler
