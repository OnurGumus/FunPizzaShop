module FunPizzaShop.Command.Domain.Order

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
open FunPizzaShop.Domain.Model.Pizza
open FunPizzaShop.Domain.Model


type Command =
    | PlaceOrder of Order
    | SetDeliveryStatus of DeliveryStatus

type Event =
    | OrderPlaced of Order
    | DeliveryStatusSet of DeliveryStatus

type State = {
    DeliveryStatus: DeliveryStatus
    Order: Order option
    Version: int64
} with

    interface IDefaultTag

let actorProp (config: IConfiguration) toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<obj>) =
    let log = Log.ForContext("OrderActor", mailbox.Self.Path.Name)
    let mediatorS = retype mediator
    let sendToSagaStarter = SagaStarter.toSendMessage mediatorS mailbox.Self

    let apply (event: Event) (state:State) =
        log.Verbose("Apply Message {@Event}, State: @{State}", event, state)

        match event with
        | OrderPlaced order -> { state with Order = Some order }
        | DeliveryStatusSet status ->
            {
                state with DeliveryStatus = status
            }

    let rec set (state: State) =
        actor {
            let! msg = mailbox.Receive()
            log.Information("Message {MSG}, State: {@State}", box msg, state)

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
                    | (PlaceOrder order) ->
                        return!
                            toEvent ci (v + 1L) (OrderPlaced order) |> sendToSagaStarter ci |> box |> Persist
                    | (SetDeliveryStatus status) ->
                        return!
                            toEvent ci (v + 1L) (DeliveryStatusSet status) |> sendToSagaStarter ci |> box |> Persist
                | _ ->
                    log.Debug("Unhandled Message {@MSG}", box msg)
                    return Unhandled
        }

    set {
        Version = 0L
        DeliveryStatus = NotDelivered
        Order = None
    }

let init (env: #_) toEvent (actorApi: IActor) =
    AkklingHelpers.entityFactoryFor actorApi.System shardResolver "Order"
    <| propsPersist (actorProp env toEvent (typed actorApi.Mediator))
    <| false

let factory (env: #_) toEvent actorApi entityId =
    (init env toEvent actorApi).RefFor DEFAULT_SHARD entityId