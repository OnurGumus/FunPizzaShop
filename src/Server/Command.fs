module FunPizzaShop.Server.Command
open FunPizzaShop.Domain.Command.Authentication

[<Interface>]
type IAuthentication =
    abstract Login: Login
    abstract Logout: Logout
    abstract Verify: Verify

[<Interface>]
type IMailSender =
    abstract SendVerificationMail: SendVerificationMail