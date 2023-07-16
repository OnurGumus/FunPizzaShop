module FunPizzaShop.MVU.SignIn

open Elmish
open FunPizzaShop.Shared.Model
open Authentication

type Status =
    | NotLoggedIn
    | LoggedIn of UserId
    | AskEmail
    | AskVerification

type Model =
    { Status: Status
      UserId: UserId option
      IsBusy: bool }

type Msg =
    | LoginRequested // Ask for email
    | LoginCancelled // cancel login
    | EmailSubmitted of UserId //email entered
    | VerificationSubmitted of VerificationCode //verification code entered
    | EmailSent
    | EmailFailed of string
    | VerificationSuccessful
    | VerificationFailed
    | LogoutRequested
    | LogoutSuccess
    | LogoutError of string

type Order =
    | NoOrder
    | Login of UserId
    | Verify of UserId * VerificationCode
    | Logout of UserId
    | ShowError of string
    | PublishLogin of UserId

val init: userName: string option -> unit -> Model * Order
val update: msg: Msg -> model: Model -> Model * Order
