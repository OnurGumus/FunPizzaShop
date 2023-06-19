[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module Command.Actor

open System.Collections.Immutable
open Akka.Streams
open Akka.Persistence.Journal
open Akka.Actor
open Akka.Cluster
open Akka.Cluster.Tools.PublishSubscribe
open Akka.Persistence.Sqlite
open Akkling
open Microsoft.Extensions.Configuration
open Common.DynamicConfig
open System.Dynamic
open FSharp.Interop.Dynamic
open System.Runtime.CompilerServices
open Microsoft.Extensions.Configuration
open System

let private defaultTag = ImmutableHashSet.Create("default")

type  Tagger =
    interface IWriteEventAdapter with
        member _.Manifest _ = ""
        member _.ToJournal evt = evt //box <| Tagged(evt, defaultTag)

    public new() = {  }


type MyEventAdapter =
    interface IEventAdapter with
        member this.FromJournal(evt: obj, manifest: string): IEventSequence =
            EventSequence.Single(evt);
        member this.Manifest(evt: obj): string = ""
        member this.ToJournal(evt: obj): obj =
           box <| Tagged(evt, defaultTag)

    public new() = {  }

[<Interface>]
type IActor =
    abstract Mediator: Akka.Actor.IActorRef
    abstract Materializer: ActorMaterializer
    abstract System: ActorSystem
    abstract SubscribeForCommand: Common.CommandHandler.Command<'a, 'b> -> Async<Common.Event<'b>>
    abstract Stop: unit -> System.Threading.Tasks.Task

let api (config: IConfiguration) =
    let (akkaConfig: ExpandoObject) =
        unbox<_> (config.GetSectionAsDynamic("config:akka"))

    let config =
        Akka.Configuration.ConfigurationFactory.FromObject akkaConfig

    let system = System.create "cluster-system" config

    SqlitePersistence.Get(system) |> ignore

    Cluster.Get(system).SelfAddress
    |> Cluster.Get(system).Join

    let mediator = DistributedPubSub.Get(system).Mediator

    let mat = ActorMaterializer.Create(system)

    let subscribeForCommand command =
        Common.CommandHandler.subscribeForCommand system (typed mediator) command

    { new IActor with
        member _.Mediator = mediator
        member _.Materializer = mat
        member _.System = system
        member _.SubscribeForCommand command = subscribeForCommand command
        member _.Stop() = system.Terminate() }