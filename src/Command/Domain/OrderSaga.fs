module  FunPizzaShop.Command.Domain.OrderSaga

open Command
open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Common
open Serilog
open Akka.Cluster.Tools.PublishSubscribe
open Actor
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Model
open Akka.Cluster.Sharding
open Thoth.Json.Net
open NodaTime
open FunPizzaShop.Shared.Model.Pizza
open Command.Common.SagaStarter
open Akka.Event

type State =
    | NotStarted
    | Started 
    | WaitingForOrderPlaced of OrderId
    | WaitingForDeliveryStart of DeliveryId * Order
    | WaitingForOrderDeliveryStatusSet of DeliveryStatus
    | Completed
   
    interface IDefaultTag

type Event =
    | StateChanged of State

    interface IDefaultTag
type SagaData = { DeliveryId : DeliveryId option; Order: Order option}
type SagaState = { Data : SagaData; State: State}

let actorProp
    (env: _)
    toEvent
    (actorApi: IActor)
    (clockInstance: IClock)
    (mediator: IActorRef<_>)
    (mailbox: Eventsourced<obj>)
    =
    let cid = (mailbox.Self.Path.Name |> SagaStarter.toCid)
    let log = mailbox.UntypedContext.GetLogger()
    let config = env :> IConfiguration
    let apiKey = config.GetSection("config:APIKEY").Value

    let rec set (state: SagaState) =

        let createCommand command = {
            CommandDetails = command
            CreationDate = clockInstance.GetCurrentInstant()
            CorrelationId = cid
        }

        let setDeliveryStatus (deliveryStatus) =
            Order.SetDeliveryStatus deliveryStatus |> createCommand

        let startDelivery (order:Order) =
            Delivery.StartDelivery order |> createCommand
      
        let orderActor (orderId: OrderId) =
            let toEvent ci = Common.toEvent clockInstance ci
            Order.factory env toEvent actorApi orderId.Value.Value

        let deliveryActor (deliveryId:DeliveryId) =
            let toEvent ci = Common.toEvent clockInstance ci
            Delivery.factory env toEvent actorApi deliveryId.Value.Value

        let apply (state:SagaState) =
            match state.State with
            | WaitingForDeliveryStart (deliveryId, order) ->
                { state.Data with DeliveryId = Some deliveryId ; Order =Some order}
            | _ -> state.Data

        let applySideEffects (state:SagaState) =
            match state.State with
            | NotStarted -> Started |> Some
            | Started ->
                SagaStarter.cont mediator
                let orderId = 
                    mailbox.Self.Path.Name 
                    |> toOriginatorName 
                    |> ShortString.TryCreate 
                    |> forceValidate |> OrderId
                (WaitingForOrderPlaced orderId) |> Some
            | WaitingForDeliveryStart (deliveryId,order) ->
                deliveryActor(deliveryId) <! startDelivery(order)
                None
            | WaitingForOrderDeliveryStatusSet (status) ->
                orderActor (state.Data.Order.Value.OrderId) <! setDeliveryStatus status
                None
            | Completed -> 
                mailbox.Parent() <! Passivate(Actor.PoisonPill.Instance)
                log.Info("OrderSaga Completed")
                None
            | WaitingForOrderPlaced _ -> None

        actor {
            let! msg = mailbox.Receive()

            log.Info(
                "OrderSaga Message {MSG}, State: {@State}, name:{@name}",
                msg,
                state,
                mailbox.Self.Path.Name
            )

            match msg with
            | :? Persistence.RecoveryCompleted ->
                SagaStarter.subscriber mediator mailbox
                return! set state
            | Recovering mailbox (:? Event as e) ->
                match e with
                | StateChanged s -> 
                    let data = apply { Data = state.Data; State = s }
                    return! set { Data = data; State = s }
                    
            | PersistentLifecycleEvent _
            | :? Akka.Persistence.SaveSnapshotSuccess
            | LifecycleEvent _ -> return! state |> set
            | SnapshotOffer(snapState: obj) ->  return! snapState |> unbox<_> |> set
            | _ ->
                match msg, state with
                | SagaStarter.SubscrptionAcknowledged mailbox _, _ ->
                    // notify saga starter about the subscription completed
                    let newState = applySideEffects state
                    match newState with
                    | Some newState ->  return!  (newState |> StateChanged |> box |> Persist)
                    | None -> return! state |> set

                | Persisted mailbox (:? Event as e), _ ->
                    match e with
                    | StateChanged originalState ->
                        let data = apply { state with  State = originalState }
                        let newSagaState = { state with Data = data}
                        let newState = applySideEffects { Data = data; State = originalState }
                        match newState with
                        | Some newState ->  return! ( newSagaState  |> set)  <@> (newState |> StateChanged |> box |> Persist)
                        | None -> return! newSagaState |> set

              
                | :? (Common.Event<Order.Event>) as {EventDetails = orderEvent;}, state ->
                    match orderEvent, state with
                    | Order.OrderPlaced order, _ ->
                        let state = WaitingForDeliveryStart (DeliveryId.CreateNew(), order) |> StateChanged
                        return! state |> box |> Persist
                    | Order.DeliveryStatusSet _, _ ->
                        let state = Completed |> StateChanged
                        return! state |> box |> Persist

                | :? (Common.Event<Delivery.Event>) as { EventDetails = deliveryEvent }, _ ->
                    match deliveryEvent with
                    | Delivery.DeliveryStarted(_) ->
                        let state =
                            WaitingForOrderDeliveryStatusSet(DeliveryStatus.OutForDelivery) |> StateChanged
                        return! state |> box |> Persist
                    | e ->
                        log.Warning("Unhandled event in calculation queue {@Event}", box e)
                        return! set state

                | e ->
                    log.Warning("Unhandled event in global {@Event}", e)
                    return! set state
        }

    set { State = NotStarted; Data = { DeliveryId = None; Order = None} }

let init (env: _) toEvent (actorApi: IActor) (clock: IClock) =
    (AkklingHelpers.entityFactoryFor actorApi.System shardResolver "OrderSaga"
     <| propsPersist (actorProp env toEvent actorApi clock (typed actorApi.Mediator))
     <| true)

let factory (env: _) toEvent actorApi clock entityId =
    (init env toEvent actorApi clock).RefFor DEFAULT_SHARD entityId
