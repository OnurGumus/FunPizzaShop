module FunPizzaShop.MVU.SignIn
open Elmish
open FunPizzaShop.Domain.Model
open Authentication

type Status = NotLoggedIn | LoggedIn of User | LoginRequested | VerificationSent 

type Model = {
    Status: Status
    User: User option
}
type Msg = LoginRequested | LoginCancelled | EmailSent | VerificationSuccessful | VerificationFailed 

type Order = NoOrder
    
let init () = { Status = NotLoggedIn; User = None } , NoOrder

let update msg model =
      match msg with
      | LoginRequested -> { model with Status =Status.LoginRequested }, NoOrder
      | LoginCancelled -> { model with Status =Status.NotLoggedIn }, NoOrder
      | EmailSent -> { model with Status =Status.VerificationSent }, NoOrder
      | VerificationSuccessful -> { model with Status =Status.LoggedIn model.User.Value }, NoOrder
      | VerificationFailed -> { model with Status =Status.NotLoggedIn }, NoOrder