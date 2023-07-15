[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module Command.Common

open System
open Akkling
open Akkling.Persistence
open Akka.Cluster.Tools.PublishSubscribe
open Akka
open Akka.Cluster.Sharding
open Akkling.Cluster.Sharding
open Akka.Actor
open Akka.Serialization
open System.IO
//open Newtonsoft.Json
open System.Text
open NodaTime.Serialization.SystemTextJson
open System.Text.Json.Serialization
open NodaTime
open Serilog

val jsonOptions: Json.JsonSerializerOptions

open System.Text.Json
open Thoth.Json.Net

type STJSerializer =
    new: system: ExtendedActorSystem -> STJSerializer
    inherit SerializerWithStringManifest
    override Identifier: int
    override ToBinary: obj: obj -> byte array
    override Manifest: o: obj -> string
    override FromBinary: bytes: byte array * manifest: string -> obj

type OriginatorName =
    | OriginatorName of string

    member Value: string

type IDefaultTag =
    interface
    end

type Command<'CommandDetails> =
    { CommandDetails: 'CommandDetails
      CreationDate: Instant
      CorrelationId: string }

    interface IDefaultTag

type Event<'EventDetails> =
    { EventDetails: 'EventDetails
      CreationDate: Instant
      CorrelationId: string
      Version: int64 }

    interface IDefaultTag

val eventEncoder: detailsEncoder: ('EventDetails -> JsonValue) -> event: Event<'EventDetails> -> JsonValue
val eventDecoder: detailsDecoder: Decoder<'EventDetails> -> Decoder<Event<'EventDetails>>
val commandEncoder: detailsEncoder: ('CommandDetails -> JsonValue) -> command: Command<'CommandDetails> -> JsonValue
val commandDecoder: detailsDecoder: Decoder<'CommandDetails> -> Decoder<Command<'CommandDetails>>
val toEvent: clockInstance: IClock -> ci: string -> version: int64 -> event: 'a -> Event<'a>

[<Literal>]
val DEFAULT_SHARD: string = "default-shard"

[<Literal>]
val SAGA_Suffix: string = "~Saga~"

[<Literal>]
val CID_Seperator: string = "~"

val shardResolver: 'a -> string
type PrefixConversion = PrefixConversion of ((string -> string) option)

module SagaStarter =
    val toOriginatorName: name: string -> string
    val toRawGuid: name: string -> string
    val toNewCid: name: string -> string
    val toCidWithExisting: name: string -> existing: string -> string
    val toCid: name: string -> string
    val cidToSagaName: name: string -> string
    val isSaga: name: string -> bool

    [<Literal>]
    val SagaStarterName: string = "SagaStarter"

    [<Literal>]
    val SagaStarterPath: string = "/user/SagaStarter"

    type Command =
        | CheckSagas of obj * originator: Actor.IActorRef * cid: string
        | Continue

    type Event = SagaCheckDone

    type Message =
        | Command of Command
        | Event of Event

    val toCheckSagas: event: 'a * originator: IActorRef * cid: string -> Message
    val toSendMessage: mediator: ICanTell<Send> -> originator: IActorRef<'a> -> cid: 'b -> event: Event<'c> -> Event<'c>
    val publishEvent: mailbox: Actor<'a> -> mediator: ICanTell<Publish> -> event: 'b -> cid: string -> unit
    val cont: mediator: ICanTell<obj> -> unit
    val subscriber: mediator: IActorRef<obj> -> mailbox: Eventsourced<'a> -> unit
    val (|SubscrptionAcknowledged|_|): context: Actor<obj> -> msg: obj -> obj option

    val actorProp:
        sagaCheck: (obj -> ((string -> IEntityRef<obj>) * PrefixConversion) list) ->
        mailbox: Actor<Message> ->
            Effect<Message>

    val init:
        system: IActorRefFactory ->
        mediator: IActorRef ->
        sagaCheck: (obj -> ((string -> IEntityRef<obj>) * PrefixConversion) list) ->
            unit

[<AutoOpen>]
module CommandHandler =
    val (|SubscriptionAcknowledged|_|): msg: obj -> SubscribeAck option

    type CommandDetails<'Command, 'Event> =
        { EntityRef: IEntityRef<obj>
          Cmd: Command<'Command>
          Filter: ('Event -> bool) }

    type State<'Command, 'Event> =
        { CommandDetails: CommandDetails<'Command, 'Event>
          Sender: IActorRef }

    type Command<'Command, 'Event> = Execute of CommandDetails<'Command, 'Event>

    val subscribeForCommand:
        system: IActorRefFactory ->
        mediator: ICanTell<obj> ->
        command: Command<'Command, 'Event> ->
            Async<Event<'Event>>

module DynamicConfig =
    open System.Runtime.CompilerServices
    open Microsoft.Extensions.Configuration
    open System.Dynamic
    open System.Collections.Generic

    val replaceWithArray: parent: ExpandoObject -> key: string -> input: ExpandoObject option -> unit
    val getSection: configs: KeyValuePair<string, 'a> seq -> obj when 'a: null

    [<Extension>]
    type ConfigExtension =
        new: unit -> ConfigExtension

        /// <summary>
        /// An extension method that returns given string as an dynamic Expando object
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown configuration or section is null</exception>
        [<Extension>]

        static member GetSectionAsDynamic: configuration: IConfiguration * section: string -> obj

        /// <summary>
        /// An extension method that returns given string as an dynamic Expando object
        /// </summary>
        /// <returns>An expando object represents given section</returns>
        /// <exception cref="System.ArgumentNullException">Thrown configuration is null</exception>
        [<Extension>]
        static member GetRootAsDynamic: configuration: IConfiguration -> obj
