module FunPizzaShop.Command.API

open Command
open Common
open Serilog
open Actor
open NodaTime
open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.Command.Domain.API
open FunPizzaShop.Domain.Command.Authentication
open FunPizzaShop.Command.Domain

let createCommandSubscription (domainApi: IDomain) factory (id: string) command filter =
    let vid =
        (id.Replace(".", "_dot_").Replace("@", "_at_").Replace("+", "_plus_"))
        |> Uri.EscapeDataString

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

module User =
    open FunPizzaShop.Domain.Model.Authentication

    let login (createSubs) : Login =
        fun userId  ->
            async {
                Log.Debug("Inside login {@userId} {@otherMeta}", userId)

                let subscribA =
                    createSubs (userId.Value) (User.Login) (function
                        | User.LoginFailed
                        | User.LoginSucceeded _ -> true
                        | _ -> false)

                let! subscrib = subscribA

                match subscrib with
                | {
                      EventDetails = User.LoginSucceeded _
                      Version = v
                  } -> return Ok()

                | {
                      EventDetails = User.LoginFailed _
                      Version = v
                  } -> return Error [ "Login failed" ]

                | other -> return failwithf "unexpected event %A" other
            }

    let verify (createSubs) : Verify =
        fun (userId, verCode) ->
            async {
                Log.Debug("Inside Verify {@userId} {@otherMeta}", userId, verCode)

                let subscribA =
                    createSubs (userId.Value) (User.VefifyLogin verCode) (function
                        | User.VerificationFailed
                        | User.VerificationSucceeded _ -> true
                        | _ -> false)

                let! subscrib = subscribA

                match subscrib with
                | {
                      EventDetails = User.VerificationSucceeded
                      Version = v
                  } -> return Ok()

                | {
                      EventDetails = User.VerificationFailed
                      Version = v
                  } -> return Error [ VerificationError.InvalidVerificationCode ]

                | other -> return failwithf "unexpected event %A" other
            }

[<Interface>]
type IAPI =
    abstract ActorApi: IActor
    abstract Login: Login
    abstract Verify: Verify


let api (env: #_) (clock: IClock) =
    let config = env :> IConfiguration
    let actorApi = Actor.api config
    let domainApi = Domain.API.api env clock actorApi
    let userSubs = createCommandSubscription domainApi domainApi.UserFactory

    { new IAPI with
        member this.Login: Login = 
              failwith "Not Implemented"
        member this.Verify: Verify = 
              User.verify userSubs
        member _.ActorApi = actorApi
    }
