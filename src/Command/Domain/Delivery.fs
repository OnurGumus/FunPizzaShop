module FunPizzaShop.Command.Domain.Delivery

open Command
open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Common
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open Actor
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Akka.Event


type Command =
    | StartDelivery of Order
    | UpdateLocation of LatLong
    | SetAsDelivered

type Event =
    | LocationUpdated of OrderId * LatLong
    | Delivered of OrderId
    | DeliveryStarted of Order

type State = {
    Order: Order option
    Version: int64
} with
    interface IDefaultTag

let actorProp (config: IConfiguration) toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<obj>) =
    let log = mailbox.UntypedContext.GetLogger()
    let mediatorS = retype mediator
    let sendToSagaStarter = SagaStarter.toSendMessage mediatorS mailbox.Self

    let apply (event: Event) (state:State) =
        log.Debug("Apply Message {@Event}, State: @{State}", event, state)

        match event with
        | LocationUpdated (_, latlong) ->
            { state with Order = Some { state.Order.Value with DeliveryLocation = latlong } }
        | Delivered _  -> { state with Order = Some { state.Order.Value with DeliveryStatus = DeliveryStatus.Delivered } }
        | DeliveryStarted order ->
            let order = { order with DeliveryStatus = DeliveryStatus.OutForDelivery }
            { state with Order = Some order }
            
    let rec set (state: State) =
        actor {
            let! msg = mailbox.Receive()
            log.Debug("Message {MSG}, State: {@State}", box msg, state)

            match msg with
            | PersistentLifecycleEvent _
            | LifecycleEvent _ -> return! state |> set

            | Persisted mailbox (:? Common.Event<Event> as event) ->
                let version = event.Version
                SagaStarter.publishEvent mailbox mediator event event.CorrelationId

                let state = {
                    (apply event.EventDetails state) with
                        Version = version
                }
                return! state |> set

            | Recovering mailbox (:? Common.Event<Event> as event) ->
                return!
                    {
                        (apply event.EventDetails state) with
                            Version = event.Version
                    }
                    |> set

            | _ ->
                match msg with
                | :? Persistence.RecoveryCompleted -> return! state |> set


                | :? (Common.Command<Command>) as msg ->

                    let ci = msg.CorrelationId 
                    let commandDetails = msg.CommandDetails
                    let v = state.Version

                    match commandDetails with
                    | (StartDelivery order) ->
                        return!
                            toEvent ci (v + 1L) (DeliveryStarted order) |> sendToSagaStarter ci |> box |> Persist

                    | (UpdateLocation (location:LatLong)) ->
                        return!
                            toEvent ci (v + 1L) (LocationUpdated (state.Order.Value.OrderId, location)) |> sendToSagaStarter ci |> box |> Persist
                    | SetAsDelivered ->
                        return!
                            toEvent ci (v + 1L) (Delivered state.Order.Value.OrderId) |> sendToSagaStarter ci |> box |> Persist
         

                | _ ->
                    log.Debug("Unhandled Message {@MSG}", box msg)
                    return Unhandled
        }

    set {
        Version = 0L
        Order = None
    }

let init (env: _) toEvent (actorApi: IActor) =
    AkklingHelpers.entityFactoryFor actorApi.System shardResolver "Delivery"
    <| propsPersist (actorProp env toEvent (typed actorApi.Mediator))
    <| false

let factory (env: _) toEvent actorApi entityId =
    (init env toEvent actorApi).RefFor DEFAULT_SHARD entityId
