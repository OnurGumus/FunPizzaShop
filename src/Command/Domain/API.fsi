module FunPizzaShop.Command.Domain.API

open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Command
open Common
open Akka.Cluster.Sharding
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open NodaTime
open Actor
open Akkling.Cluster.Sharding
open Microsoft.Extensions.Configuration

val sagaCheck:
    env: #IConfiguration ->
    toEvent: 'b ->
    actorApi: IActor ->
    clock: IClock ->
    o: obj ->
        ((string -> IEntityRef<obj>) * PrefixConversion) list

[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract UserFactory: string -> IEntityRef<obj>
    abstract OrderFactory: string -> IEntityRef<obj>
    abstract DeliveryFactory: string -> IEntityRef<obj>

val api:
    env: 'a -> clock: IClock -> actorApi: IActor -> IDomain
        when 'a :> IConfiguration and 'a :> FunPizzaShop.ServerInterfaces.Command.IMailSender
