[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module AkklingHelpers

open Akka.Actor
open Akka.Cluster.Sharding
open Akkling
open Akkling.Persistence
open Akkling.Cluster.Sharding

type Extractor<'Envelope, 'Message> = 'Envelope -> string * string * 'Message
type ShardResolver = string -> string

type internal TypedMessageExtractor<'Envelope, 'Message> =
    internal new:
        extractor: Extractor<'Envelope, 'Message> * shardResolver: ShardResolver ->
            TypedMessageExtractor<'Envelope, 'Message>

    interface IMessageExtractor

type FunPersistentShardingActor<'Message> =
    new: actor: (Eventsourced<'Message> -> Effect<'Message>) -> FunPersistentShardingActor<'Message>
    inherit FunPersistentActor<'Message>
    override PersistenceId: string

val internal adjustPersistentProps: props: Props<'Message> -> Props<'Message>

val entityFactoryFor:
    system: ActorSystem ->
    shardResolver: ShardResolver ->
    name: string ->
    props: Props<'Message> ->
    rememberEntities: bool ->
        EntityFac<'Message>

val (|Recovering|_|): context: Eventsourced<'Message> -> msg: 'Message -> 'Message option
