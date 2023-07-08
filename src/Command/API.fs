module FunPizzaShop.Command.API

open Command
open Common
open Serilog
open Actor
open NodaTime
open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.Command.Domain.API
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza
open FunPizzaShop.Command.Domain
open FunPizzaShop.Shared.Model.Pizza

let createCommandSubscription (domainApi: IDomain) factory (id: string) command filter =
    let corID = id |> Uri.EscapeDataString |> SagaStarter.toNewCid
    let actor = factory id

    let commonCommand: Command<_> = {
        CommandDetails = command
        CreationDate = domainApi.Clock.GetCurrentInstant()
        CorrelationId = corID
    }

    let e = {
        Cmd = commonCommand
        EntityRef = actor
        Filter = filter
    }

    let ex = Execute e
    ex |> domainApi.ActorApi.SubscribeForCommand

module User =
    open FunPizzaShop.Shared.Model.Authentication

    let login (createSubs) : Login =
        fun userId  ->
            async {
                Log.Debug("Inside login {@userId}", userId)

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
                Log.Debug("Inside Verify {@userId} {@verCode}", userId, verCode)

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

module Pizza =
    open FunPizzaShop.Shared.Model.Pizza

    let order (createSubs) : OrderPizza =
        fun order  ->
            async {
                Log.Debug("Inside order {@order}", order)

                let subscribA =
                    createSubs (order.OrderId.Value.Value) (Order.PlaceOrder order) (function
                        | Order.OrderPlaced _ ->true
                        | _ -> false)

                let! subscrib = subscribA

                match subscrib with
                | {
                      EventDetails = Order.OrderPlaced _
                      Version = v
                  } -> return ()

                | other -> return failwithf "unexpected event %A" other
            }

   

[<Interface>]
type IAPI =
    abstract ActorApi: IActor
    abstract Login: Login
    abstract Verify: Verify
    abstract OrderPizza: OrderPizza


let api (env: #_) (clock: IClock) =
    let config = env :> IConfiguration
    let actorApi = Actor.api config
    let domainApi = Domain.API.api env clock actorApi
    let userSubs =  createCommandSubscription domainApi domainApi.UserFactory
    let pizzaSubs = createCommandSubscription domainApi domainApi.OrderFactory
    { new IAPI with
        member this.Login: Login = 
              User.login userSubs
        member this.Verify: Verify = 
              User.verify userSubs
        member _.ActorApi = actorApi
        member _.OrderPizza = 
              Pizza.order pizzaSubs
    }
