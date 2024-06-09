module  FunPizzaShop.Command.Domain.DeliverySaga

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
open System
open Akkling.Cluster.Sharding
open Akka.Event

type State =
    | NotStarted
    | Started of (Common.Event<Delivery.Event>) 
    | WaitingForDeliveryStart of DeliveryId
    | WaitingForOrderDeliveryCompleted  of OrderId
    | WaitingForOrderDeliveryStatusSet
    | Completed
   
    interface IDefaultTag

type Event =
    | StateChanged of State

    interface IDefaultTag
type SagaData = { DeliveryId : DeliveryId option; Order: OrderId option}
type SagaState = { Data : SagaData; State: State}
let initialState = { State = NotStarted; Data = { DeliveryId = None; Order = None} }
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

    let rec set (state: SagaState) =

        let createCommand command = {
            CommandDetails = command
            CreationDate = clockInstance.GetCurrentInstant()
            CorrelationId = cid
        }

        let setDeliveryStatusForOrder (deliveryStatus) =
            Order.SetDeliveryStatus deliveryStatus |> createCommand

        let completeDelivery () =
            Delivery.SetAsDelivered |> createCommand

        let updateLocation (latLong) =
            Delivery.UpdateLocation(latLong )  |> createCommand
      
        let orderActor (orderId: OrderId) =
            let toEvent ci = Common.toEvent clockInstance ci
            Order.factory env toEvent actorApi orderId.Value.Value

        let deliveryActor (deliveryId:DeliveryId) =
            let toEvent ci = Common.toEvent clockInstance ci
            Delivery.factory env toEvent actorApi deliveryId.Value.Value

        let apply (state:SagaState) =
            match state.State with
            | WaitingForDeliveryStart (deliveryId) ->
                { state.Data with DeliveryId = Some deliveryId }
            | WaitingForOrderDeliveryCompleted (orderId) ->
                { state.Data with Order = Some orderId }
            | WaitingForOrderDeliveryStatusSet ->
                state.Data
            | _ -> state.Data

        let applySideEffects (state:SagaState) recovered =
            match state.State with
            | NotStarted ->  None

            | Started _ ->
                if recovered then 
                    Some Completed
                else
                SagaStarter.cont mediator
                let deliveryId = 
                    mailbox.Self.Path.Name 
                    |> toOriginatorName 
                    |> ShortString.TryCreate 
                    |> forceValidate |> DeliveryId
                (WaitingForDeliveryStart deliveryId) |> Some

            | WaitingForDeliveryStart _ ->
                if recovered then
                    Some (Completed)
                else
                    None

            | WaitingForOrderDeliveryCompleted _ ->
                let deliveryId = state.Data.DeliveryId.Value
                let target =  (deliveryActor(deliveryId))
                let shard = ClusterSharding.Get(mailbox.System).ShardRegion("Delivery")       

                let message = { 
                    ShardId = target.ShardId; 
                    EntityId = target.EntityId; 
                    Message = completeDelivery() } :ShardEnvelope
                mailbox.System.Scheduler.
                    ScheduleTellOnceCancelable((TimeSpan.FromSeconds(20)), shard, message, mailbox.UntypedContext.Self) 
                    |> ignore
                for i = 0 to 5 do 
                    let latLong = { Latitude = (i * 10)|> float; Longitude = (i * 10) |> float }:LatLong
                    let message = { 
                        ShardId = target.ShardId; 
                        EntityId = target.EntityId; 
                        Message = updateLocation(latLong) } :ShardEnvelope
                    mailbox.System.Scheduler.
                        ScheduleTellOnceCancelable (TimeSpan.FromSeconds(3. * float(i)), shard, message, mailbox.UntypedContext.Self) 
                        |> ignore
                    
               
                None
            | WaitingForOrderDeliveryStatusSet ->
                orderActor(state.Data.Order.Value) <! setDeliveryStatusForOrder(DeliveryStatus.Delivered)
                None
            | Completed -> 
                if recovered then
                    failwith    "Completed"
                else
                mailbox.Parent() <! Akka.Cluster.Sharding.Passivate(Actor.PoisonPill.Instance)
                log.Info("DeliverySaga Completed")
                None
        actor {
            let! msg = mailbox.Receive()

            log.Info(
                "DeliverySaga Message {MSG}, State: {@State}, name:{@name}",
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
                    let newState = applySideEffects state true
                    match newState with
                    | Some newState ->  return!  (newState |> StateChanged |> box |> Persist)
                    | None -> return! state |> set

                | Persisted mailbox (:? Event as e), _ ->
                    match e with
                    | StateChanged originalState ->
                        let data = apply { state with  State = originalState }
                        let newSagaState = { state with Data = data}
                        let newState = applySideEffects { Data = data; State = originalState } false
                        match newState with
                        | Some newState ->  return! ( newSagaState  |> set)  <@> (newState |> StateChanged |> box |> Persist)
                        | None -> return! newSagaState |> set

              
                | :? (Common.Event<Order.Event>) as {EventDetails = orderEvent;}, state ->
                    match orderEvent, state with
                    | Order.DeliveryStatusSet _, _ ->
                        let state = Completed |> StateChanged
                        return! state |> box |> Persist
                    | e ->
                        log.Warning("Unhandled event in delivery saga {@Event}", box e)
                        return! set state
                    

                | :? (Common.Event<Delivery.Event> ) as ({ EventDetails = deliveryEvent }as de) , _ ->
                    
                    match state.State, deliveryEvent with
                    // | NotStarted,w->
                    //     log.Warning("Start event {@e}", box w)
                    //     return! set state
                    | _,Delivery.DeliveryStarted(order) when state.Data.DeliveryId.IsSome ->
                        let state =
                            WaitingForOrderDeliveryCompleted order.OrderId |> StateChanged
                        return! state |> box |> Persist
                    | _,Delivery.Delivered _ ->
                        let state =
                            WaitingForOrderDeliveryStatusSet  |> StateChanged
                        return! state |> box |> Persist
                    | _, Delivery.LocationUpdated _ ->
                        return! set state
                    | State.NotStarted, _ ->
                        let state = Started  de |> StateChanged
                        return! state |> box |> Persist
                    | e ->
                        log.Warning("Unhandled event in delivery saga {@Event}", box e)
                        return! set state

                | e ->
                    log.Warning("Unhandled event in global {@Event}", e)
                    return! set state
        }

    set initialState

let init (env: _) toEvent (actorApi: IActor) (clock: IClock) =
    (AkklingHelpers.entityFactoryFor actorApi.System shardResolver "DeliverySaga"
     <| propsPersist (actorProp env toEvent actorApi clock (typed actorApi.Mediator))
     <| true)

let factory (env: _) toEvent actorApi clock entityId =
    (init env toEvent actorApi clock).RefFor DEFAULT_SHARD entityId
