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

let sagaCheck (env:#_) toEvent actorApi (clock: IClock) (o: obj) =
    match o with
    | _ -> []

[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock

let api (env: #_) (clock: IClock) (actorApi: IActor) =

    let toEvent ci = Common.toEvent clock ci
    SagaStarter.init actorApi.System actorApi.Mediator (sagaCheck env toEvent actorApi clock)

    |> sprintf "CalculationRegistrationSaga initialized %A"
    |> Log.Debug

    // EmailService.init actorApi.System actorApi.Mediator sendEmail
    System.Threading.Thread.Sleep(1000)

    { new IDomain with
        member _.Clock = clock
        member _.ActorApi = actorApi
    }
