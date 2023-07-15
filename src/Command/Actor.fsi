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

[<Class>]
type Tagger =
    interface IWriteEventAdapter
    public new: unit -> Tagger

[<Class>]
type MyEventAdapter =
    interface IEventAdapter
    public new: unit -> MyEventAdapter

[<Interface>]
type IActor =
    abstract Mediator: Akka.Actor.IActorRef
    abstract Materializer: ActorMaterializer
    abstract System: ActorSystem
    abstract SubscribeForCommand: Common.CommandHandler.Command<'a, 'b> -> Async<Common.Event<'b>>
    abstract Stop: unit -> System.Threading.Tasks.Task

val api: config: IConfiguration -> IActor
