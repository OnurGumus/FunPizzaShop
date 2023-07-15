module FunPizzaShop.Command.Domain.OrderSaga

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

type SagaData =
    { DeliveryId: DeliveryId option
      Order: Order option }

type SagaState = { Data: SagaData; State: State }

val actorProp:
    env: #IConfiguration ->
    toEvent: 'b ->
    actorApi: IActor ->
    clockInstance: IClock ->
    mediator: IActorRef<obj> ->
    mailbox: Eventsourced<obj> ->
        Effect<obj>

val init: env: #IConfiguration -> toEvent: 'b -> actorApi: IActor -> clock: IClock -> Cluster.Sharding.EntityFac<obj>

val factory:
    env: #IConfiguration ->
    toEvent: 'b ->
    actorApi: IActor ->
    clock: IClock ->
    entityId: string ->
        Cluster.Sharding.IEntityRef<obj>
