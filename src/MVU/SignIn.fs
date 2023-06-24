module FunPizzaShop.MVU.SignIn
open Elmish
open FunPizzaShop.Domain.Model
open Authentication

type Status = NotLoggedIn | LoggedIn of User | AskEmail | AskVerification 

type Model = {
    Status: Status
    User: User option
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

let init () = { Status = NotLoggedIn; User = None ; IsBusy = false} , NoOrder

let update msg model =
      match msg with
        | LoginRequested -> { model with Status =Status.AskEmail }, NoOrder
        | LoginCancelled -> { model with Status =Status.NotLoggedIn }, NoOrder
        | EmailSubmitted email -> 
            model, Order.Login email
        | EmailSent -> { model with Status =Status.AskVerification }, NoOrder
        | VerificationSubmitted code -> 
            model, Order.Verify (model.User.Value.Id, code)
        | EmailFailed ex -> { model with Status =Status.NotLoggedIn }, Order.ShowError ex
        | VerificationSuccessful -> { model with Status =Status.LoggedIn model.User.Value }, NoOrder
        | VerificationFailed -> { model with Status =Status.NotLoggedIn }, NoOrder
        | LogoutSuccess -> { model with Status =Status.NotLoggedIn }, NoOrder
        | LogoutError ex -> { model with Status =Status.NotLoggedIn }, Order.ShowError ex
        