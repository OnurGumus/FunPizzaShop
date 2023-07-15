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
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open Akka.Event

type Command =
    | PlaceOrder of Order
    | SetDeliveryStatus of DeliveryStatus

type Event =
    | OrderPlaced of Order
    | DeliveryStatusSet of OrderId * DeliveryStatus

type State =
    { DeliveryStatus: DeliveryStatus
      Order: Order option
      Version: int64 }

    interface IDefaultTag

val actorProp:
    config: IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'a>) ->
    mediator: IActorRef<Publish> ->
    mailbox: Eventsourced<obj> ->
        Effect<obj>

val init:
    env: #IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'b>) ->
    actorApi: IActor ->
        Cluster.Sharding.EntityFac<obj>

val factory:
    env: #IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'b>) ->
    actorApi: IActor ->
    entityId: string ->
        Cluster.Sharding.IEntityRef<obj>
