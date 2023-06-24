module FunPizzaShop.Command.Domain.User

open Command
open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Common
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open Actor
open Microsoft.Extensions.Configuration
open FunPizzaShop.Domain.Model.Authentication
open FunPizzaShop.Domain.Model


type Command =
    | Login
    | VefifyLogin of VerificationCode option

type Event =
    | LoginSucceeded of VerificationCode option
    | LoginFailed
    | VerificationFailed
    | VerificationSucceeded

let random = System.Random.Shared


type State = {
    Verification: VerificationCode option
    Version: int64
    
} with

    interface IDefaultTag

let actorProp (config: IConfiguration) toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<obj>) =
    let log = Log.ForContext("UserActor", mailbox.Self.Path.Name)
    let mediatorS = retype mediator
    let sendToSagaStarter = SagaStarter.toSendMessage mediatorS mailbox.Self

    let apply (event: Event) (state:State) =
        log.Verbose("Apply Message {@Event}, State: @{State}", event, state)

        match event with
        | LoginSucceeded(code) ->
            {
                state with
                    Verification = code
            }
       
        | _ -> state

    let rec set (state: State) =
        actor {
            let! msg = mailbox.Receive()
            log.Information("Message {MSG}, State: {@State}", box msg, state)

            match msg with
            | PersistentLifecycleEvent _
            | :? Persistence.SaveSnapshotSuccess
            | LifecycleEvent _ -> return! state |> set

            | SnapshotOffer(snapState: obj) -> return! snapState |> unbox<_> |> set

            | Persisted mailbox (:? Common.Event<Event> as event) ->
                let version = event.Version
                SagaStarter.publishEvent mailbox mediator event event.CorrelationId

                let state = {
                    (apply event.EventDetails state) with
                        Version = version
                }

                if (version >= 30L && version % 30L = 0L) then
                    return! state |> set <@> SaveSnapshot(state)
                else
                    return! state |> set

            | Recovering mailbox (:? Common.Event<Event> as event) ->
                return!
                    {
                        (apply event.EventDetails state) with
                            Version = event.Version
                    }
                    |> set

            | _ ->
                match msg with
                | :? Persistence.RecoveryCompleted -> return! state |> set


                | :? (Common.Command<Command>) as userMsg ->

                    let ci = userMsg.CorrelationId
                    let commandDetails = userMsg.CommandDetails
                    let v = state.Version

                    match commandDetails with
                    | (VefifyLogin incomingCode) ->
                        let verficiationEvent =
                            if mailbox.Pid.Contains(@"_at_") then
                                if incomingCode.IsNone then VerificationSucceeded
                                else
                                match state.Verification with
                                | Some(code) when code = incomingCode.Value -> VerificationSucceeded
                                | _ -> VerificationFailed
                            else
                                let lastSlash = mailbox.Pid.LastIndexOf("/")

                                let id =
                                    mailbox.Pid
                                        .Substring(lastSlash + 1)
                                        .Replace("_at_", "@")
                                        .Replace("_dot_", ".")
                                        .Replace("_plus_", "+")

                                if
                                    //(BestFitBox.Command.MailSender.checkSMSVerification config id incomingCode.Value.Value) = "approved"
                                    false
                                then
                                    VerificationSucceeded
                                else
                                    VerificationFailed

                        let verficiationOutcome =
                            toEvent ci (v + 1L) verficiationEvent |> sendToSagaStarter ci |> box |> Persist

                      

                        return! verficiationOutcome

                    
                    | (Login) ->
                        try
                            let verificationCode =
                                VerificationCode.TryCreate(random.Next(100000, 999999).ToString())
                                |> forceValidate

                            let lastSlash = mailbox.Pid.LastIndexOf("/")

                            let id =
                                mailbox.Pid
                                    .Substring(lastSlash + 1)
                                    .Replace("_at_", "@")
                                    .Replace("_dot_", ".")
                                    .Replace("_plus_", "+")

                            // BestFitBox.Command.MailSender.sendMessage
                            //     config
                            //     id
                            //     "Verification Code"
                            //     $"Your verification code is <b>{verificationCode.Value}</b>"
                            //     verificationCode.Value

                            let e = LoginSucceeded( Some verificationCode)
                            return! toEvent ci v e |> box |> Persist
                        with ex ->
                            Log.Error(ex, "Error sending verification code")
                            let e2 = LoginFailed
                            return! toEvent ci v e2 |> box |> Persist

                    

                | _ ->
                    log.Debug("Unhandled Message {@MSG}", box msg)
                    return Unhandled
        }

    set {
        Version = 0L
        Verification = None
    }

let init (env: #_) toEvent (actorApi: IActor) =
    AkklingHelpers.entityFactoryFor actorApi.System shardResolver "User"
    <| propsPersist (actorProp env toEvent (typed actorApi.Mediator))
    <| false

let factory (env: #_) toEvent actorApi entityId =
    (init env toEvent actorApi).RefFor DEFAULT_SHARD entityId
