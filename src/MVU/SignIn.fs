module FunPizzaShop.MVU.SignIn
open Elmish
open FunPizzaShop.Domain.Model
open Authentication

type Status = NotLoggedIn | LoggedIn of UserId | AskEmail | AskVerification 

type Model = {
    Status: Status
    UserId: UserId option
    IsBusy : bool;
}
type Msg = 
    | LoginRequested // Ask for email
    | LoginCancelled  // cancel login
    | EmailSubmitted of UserId //email entered
    | VerificationSubmitted of VerificationCode //verification code entered
    | EmailSent 
    | EmailFailed of string
    | VerificationSuccessful 
    | VerificationFailed 
    | LogoutSuccess
    | LogoutError of string

type Order = 
    | NoOrder
    | Login of UserId
    | Verify of UserId * VerificationCode
    | Logout of UserId
    | ShowError of string

let init () = { Status = NotLoggedIn; UserId = None ; IsBusy = false} , NoOrder

let update msg model =
      match msg with
        | LoginRequested -> { model with Status =Status.AskEmail }, NoOrder
        | LoginCancelled -> { model with Status =Status.NotLoggedIn }, NoOrder
        | EmailSubmitted email -> 
            {model with UserId =  Some email }, Order.Login email
        | EmailSent -> { model with Status =Status.AskVerification }, NoOrder
        | VerificationSubmitted code -> 
            model, Order.Verify (model.UserId.Value, code)
        | EmailFailed ex -> model, Order.ShowError ex
        | VerificationSuccessful -> { model with Status =Status.LoggedIn model.UserId.Value }, NoOrder
        | VerificationFailed -> model,  Order.ShowError "Verification failed"
        | LogoutSuccess -> { model with Status =Status.NotLoggedIn }, NoOrder
        | LogoutError ex -> { model with Status =Status.NotLoggedIn }, Order.ShowError ex
        