module FunPizzaShop.Command.API

open Command
open Common
open Serilog
open Actor
open NodaTime
open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.Command.Domain.API

let createCommandSubscription (domainApi: IDomain) factory (id: string) command filter =
    let vid = (id.Replace(".", "_dot_").Replace("@", "_at_").Replace("+", "_plus_")) |> Uri.EscapeDataString

    let corID = vid |> SagaStarter.toNewCid
    let visitorActor = factory vid

    let commonCommand: Command<_> = {
        CommandDetails = command
        CreationDate = domainApi.Clock.GetCurrentInstant()
        CorrelationId = corID
    }

    let e = {
        Cmd = commonCommand
        EntityRef = visitorActor
        Filter = filter
    }

    let ex = Execute e
    ex |> domainApi.ActorApi.SubscribeForCommand

[<Interface>]
type IAPI =
    abstract ActorApi: IActor


let api (env: #_) (clock: IClock) =
    let config = env :> IConfiguration
    let actorApi = Actor.api config
    let domainApi = Domain.API.api env clock actorApi

    { new IAPI with
        member _.ActorApi = actorApi
    }
