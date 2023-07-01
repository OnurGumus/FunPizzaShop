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

let jsonOptions =
    Json
        .JsonSerializerOptions(
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        )
        .ConfigureForNodaTime((DateTimeZoneProviders.Tzdb))

jsonOptions.Converters.Add(
    JsonFSharpConverter(
        unionEncoding =
            (JsonUnionEncoding.UnwrapSingleFieldCases
             ||| JsonUnionEncoding.UnwrapFieldlessTags
             ||| JsonUnionEncoding.UnwrapOption
             ||| JsonUnionEncoding.ExternalTag
             ||| JsonUnionEncoding.NamedFields
             ||| JsonUnionEncoding.UnwrapSingleCaseUnions),
        unionTagCaseInsensitive = true
    )
)

open System.Text.Json
open Thoth.Json.Net

type STJSerializer(system: ExtendedActorSystem) =
    inherit SerializerWithStringManifest(system)
    do ()

    override __.Identifier = 1711

    override __.ToBinary(o) =

        let memoryStream = new MemoryStream()
        JsonSerializer.Serialize(memoryStream, o, jsonOptions)
        memoryStream.ToArray()

    override this.Manifest(o: obj) : string = o.GetType().FullName

    override this.FromBinary(bytes: byte[], manifest: string) : obj =
        JsonSerializer.Deserialize(new MemoryStream(bytes), Type.GetType(manifest), jsonOptions)

type OriginatorName = 
    | OriginatorName of string
    member this.Value = let (OriginatorName on) = this in on

type IDefaultTag =
    interface
    end

type Command<'CommandDetails> = {
    CommandDetails: 'CommandDetails
    CreationDate: Instant
    CorrelationId: string
} with

    interface IDefaultTag

type Event<'EventDetails> = {
    EventDetails: 'EventDetails
    CreationDate: Instant
    CorrelationId: string
    Version: int64
} with

    interface IDefaultTag

let eventEncoder (detailsEncoder) (event: Event<'EventDetails>) =
    Encode.object [
        "correlationid", Encode.string event.CorrelationId
        "eventDetails", detailsEncoder event.EventDetails
        "version", Encode.int64 event.Version
    ]

let eventDecoder (detailsDecoder) : Decoder<Event<'EventDetails>> =
    Decode.object (fun get -> {
        EventDetails = get.Required.Field "eventDetails" detailsDecoder
        Version = get.Required.Field "version" Decode.int64
        CorrelationId = get.Required.Field "version" Decode.string
        CreationDate = NodaTime.SystemClock.Instance.GetCurrentInstant()
    })

let commandEncoder (detailsEncoder) (command: Command<'CommandDetails>) =
    Encode.object [
        "correlationid", Encode.string command.CorrelationId
        "commandDetails", detailsEncoder command.CommandDetails
    ]

let commandDecoder (detailsDecoder) : Decoder<Command<'CommandDetails>> =
    Decode.object (fun get -> {
        CommandDetails = get.Required.Field "commandDetails" detailsDecoder
        CorrelationId = get.Required.Field "version" Decode.string
        CreationDate = NodaTime.SystemClock.Instance.GetCurrentInstant()
    })

let toEvent (clockInstance: IClock) ci version event = {
    EventDetails = event
    CreationDate = clockInstance.GetCurrentInstant()
    CorrelationId = ci
    Version = version
}

[<Literal>]
let DEFAULT_SHARD = "default-shard"

let shardResolver = fun _ -> DEFAULT_SHARD

module SagaStarter =

    let removeSaga (name: string) =
        let first = name.Replace("_Saga_","")
        let index = first.IndexOf('_')
      //  let lastIndex = first.LastIndexOf('_')

        if index >0 then
            first.Substring(index + 1)
        else
            first
      //  name.Replace("_Saga_","")

    let toOriginatorName (name: string) =
        let sagaRemoved = removeSaga name
        let bang = sagaRemoved.IndexOf('~')
        sagaRemoved.Substring(0, bang)

    let toRawGuid (name: string) =
        let name  = name.Replace("_Saga_","")
        let index = name.IndexOf('~')
        name.Substring(index + 1)

    let toNewCid name = name + "~" + Guid.NewGuid().ToString()

    let toCidWithExisting (name: string) (existing: string) =
        let originator = name
        let guid = existing |> toRawGuid
        originator + "~" + guid

    let toCid name =
        let originator = (name |> toOriginatorName)
        let guid = name |> toRawGuid
        originator + "~" + guid

    let cidToSagaName (name: string) = name + "_Saga_"
    let isSaga (name: string) = name.Contains("_Saga_")

    [<Literal>]
    let SagaStarterName = "SagaStarter"

    [<Literal>]
    let SagaStarterPath = "/user/SagaStarter"

    type Command =
        | CheckSagas of obj * originator: Actor.IActorRef * cid: string
        | Continue

    type Event = SagaCheckDone

    type Message =
        | Command of Command
        | Event of Event

    let toCheckSagas (event, originator, cid) =
        ((event |> box), originator, cid) |> CheckSagas |> Command

    let toSendMessage mediator (originator:IActorRef<_>) cid event =
        let cid = toCidWithExisting (originator.Path.Name) event.CorrelationId 
        let message =
            Send(SagaStarterPath, (event, untyped originator, cid) |> toCheckSagas)

        (mediator <? (message)) |> Async.RunSynchronously |> ignore
        event

    let publishEvent (mailbox: Actor<_>) (mediator) event (cid) =
        let sender = mailbox.Sender()
        let self = mailbox.Self
        Log.Debug("Publishing event {event} to {self}", event, self.Path.Name)

        if sender.Path.Name |> isSaga then
            let originatorName = sender.Path.Name |> toOriginatorName

            if originatorName <> self.Path.Name then
                sender <! (event)

        mediator <! Publish(self.Path.Name, event)
        mediator <! Publish(self.Path.Name + "~" + cid, event)

    let cont (mediator) =
        mediator <! box (Send(SagaStarterPath, Continue |> Command,true))

    let subscriber (mediator: IActorRef<_>) (mailbox: Eventsourced<_>) =
        let originatorName = mailbox.Self.Path.Name |> toOriginatorName

        mediator <! box (Subscribe(originatorName, untyped mailbox.Self))

    let (|SubscrptionAcknowledged|_|) (context: Actor<obj>) (msg: obj) : obj option =
        let originatorName = context.Self.Path.Name |> toOriginatorName

        match msg with
        | :? SubscribeAck as s when s.Subscribe.Topic = originatorName -> Some msg
        | _ -> None

    let actorProp (sagaCheck: obj -> (((string -> IEntityRef<_>) * string) list)) (mailbox: Actor<_>) =
        let rec set (state: Map<string, (Actor.IActorRef * string list)>) =

            let startSaga cid (originator: Actor.IActorRef) (list: ((string -> IEntityRef<_>) * string) list) =
                let sender = untyped <| mailbox.Sender()

                let sagas = [
                    for (factory, prefix) in list do
                        let saga =
                            cid
                            |> cidToSagaName
                            |> fun name ->
                                match prefix with
                                | null
                                | "" -> name
                                | other -> sprintf "%s_%s" other name
                            |> factory

                        saga <! box (ShardRegion.StartEntity(saga.EntityId))

                        yield saga.EntityId
                ]

                let name = originator.Path.Name

                let state =
                    match state.TryFind(name) with
                    | None -> state.Add(name, (sender, sagas))
                    | Some(_, list) -> state.Remove(name).Add(name, (sender, list @ sagas))

                state

            actor {
                match! mailbox.Receive() with
                | Command(Continue) ->
                    //check if all sagas are started. if so issue SagaCheckDone to originator else keep wait
                    let sender = untyped <| mailbox.Sender()
                    let originName = sender.Path.Name |> toOriginatorName
                    //weird bug cause an NRE with TryGet
                    let matchFound = state.ContainsKey(originName)

                    if not matchFound then
                        return! set state
                    else
                        let (originator, subscribers) = state.[originName]
                        let newList = subscribers |> List.filter (fun a -> a <> sender.Path.Name)

                        match newList with
                        | [] -> originator.Tell(SagaCheckDone, untyped mailbox.Self)
                        | _ -> return! set <| state.Remove(originName).Add(originName, (originator, newList))


                | Command(CheckSagas(o, originator, cid)) ->
                    match sagaCheck o with
                    | [] ->
                        mailbox.Sender() <! SagaCheckDone
                        return! set state
                    | list -> return! set <| startSaga cid originator list

                | _ -> return! Unhandled
            }
        set Map.empty

    let init system mediator sagaCheck =
        let sagaStarter = spawn system <| SagaStarterName <| props (actorProp sagaCheck)
        typed mediator <! (sagaStarter |> untyped |> Put)

[<AutoOpen>]
module CommandHandler =

    let (|SubscriptionAcknowledged|_|) (msg: obj) =
        match msg with
        | :? SubscribeAck as s -> Some s
        | _ -> None

    type CommandDetails<'Command, 'Event> = {
        EntityRef: IEntityRef<obj>
        Cmd: Command<'Command>
        Filter: ('Event -> bool)
    }

    type State<'Command, 'Event> = {
        CommandDetails: CommandDetails<'Command, 'Event>
        Sender: IActorRef
    }

    type Command<'Command, 'Event> = Execute of CommandDetails<'Command, 'Event>

    let subscribeForCommand<'Command, 'Event> system mediator (command: Command<'Command, 'Event>) =
        let actorProp mediator (mailbox: Actor<obj>) =
            let rec set (state: State<'Command, 'Event> option) =
                actor {
                    let! msg = mailbox.Receive()

                    match box msg with
                    | SubscriptionAcknowledged _ ->
                        let cmd = (state.Value.CommandDetails.Cmd) |> box
                        state.Value.CommandDetails.EntityRef <! cmd

                        return! set state
                    | :? Command<'Command, 'Event> as s ->
                        let sender = mailbox.Sender()

                        let cd =
                            match s with
                            | Execute cd ->
                                mediator
                                <! box (
                                    Subscribe(cd.EntityRef.EntityId + "~" + cd.Cmd.CorrelationId, untyped mailbox.Self)
                                )
                                cd

                        return!
                            Some {
                                CommandDetails = cd
                                Sender = untyped sender
                            }
                            |> set

                    | :? (Event<'Event>) as e when e.CorrelationId = state.Value.CommandDetails.Cmd.CorrelationId ->
                        if state.Value.CommandDetails.Filter e.EventDetails then
                            state.Value.Sender.Tell e
                            return! Stop
                        else
                            Log.Debug("Ignoring from subscriber message {msg}", msg)
                            return! set state
                    | LifecycleEvent _ -> return! Ignore
                    | _ ->
                        Log.Error("Unexpected message {msg}", msg)
                        return! Ignore
                }

            set None

        async {
            let! res = spawnAnonymous system (props (actorProp mediator)) <? box command
            return box res :?> Event<'Event>
        }

module DynamicConfig =
    open System.Runtime.CompilerServices
    open Microsoft.Extensions.Configuration
    open System.Dynamic
    open System.Collections.Generic

    let rec replaceWithArray (parent: ExpandoObject) (key: string) (input: ExpandoObject option) =
        match input with
        | None -> ()
        | Some input ->
            let dict = input :> IDictionary<_, _>
            let keys = dict.Keys |> List.ofSeq

            if keys |> Seq.forall (Int32.TryParse >> fst) then
                let arr = keys.Length |> Array.zeroCreate

                for kvp in dict do
                    arr.[kvp.Key |> Int32.Parse] <- kvp.Value

                let parentDict = parent :> IDictionary<_, _>
                parentDict.Remove key |> ignore
                parentDict.Add(key, arr)
            else
                for childKey in keys do
                    let newInput =
                        match dict.[childKey] with
                        | :? ExpandoObject as e -> Some e
                        | _ -> None

                    replaceWithArray input childKey newInput

    let getSection (configs: KeyValuePair<string, _> seq) : obj =
        let result = ExpandoObject()

        for kvp in configs do
            let mutable parent = result :> IDictionary<_, _>
            let path = kvp.Key.Split(':')
            let mutable i = 0

            while i < path.Length - 1 do
                if parent.ContainsKey(path.[i]) |> not then
                    parent.Add(path.[i], ExpandoObject())

                parent <- downcast parent.[path.[i]]
                i <- i + 1

            if kvp.Value |> isNull |> not then
                parent.Add(path.[i], kvp.Value)

        replaceWithArray null null (Some result)
        upcast result

    [<Extension>]
    type ConfigExtension() =
        /// <summary>
        /// An extension method that returns given string as an dynamic Expando object
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown configuration or section is null</exception>
        [<Extension>]

        static member GetSectionAsDynamic(configuration: IConfiguration, section: string) : obj =
            if configuration |> isNull then
                "configuration" |> ArgumentNullException |> raise

            if section |> isNull then
                "section" |> ArgumentNullException |> raise

            let configs =
                configuration.GetSection(section).AsEnumerable()
                |> Seq.filter (fun k -> k.Key.StartsWith(sprintf "%s:" section))

            let res = getSection configs

            let paths = section.Split(":", StringSplitOptions.None) |> List.ofArray

            let rec loop paths (res: obj) =
                match paths, res with
                | head :: (_ :: _ as tail), (:? IDictionary<string, obj> as d) ->
                    let v = d.[head]
                    loop tail v
                | _ -> res

            loop paths res

        /// <summary>
        /// An extension method that returns given string as an dynamic Expando object
        /// </summary>
        /// <returns>An expando object represents given section</returns>
        /// <exception cref="System.ArgumentNullException">Thrown configuration is null</exception>
        [<Extension>]
        static member GetRootAsDynamic(configuration: IConfiguration) : obj =
            if configuration |> isNull then
                "configuration" |> ArgumentNullException |> raise

            let configs = configuration.AsEnumerable()
            getSection configs
