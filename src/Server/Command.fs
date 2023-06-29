module FunPizzaShop.Server.Command
open FunPizzaShop.Domain.Command.Authentication
open FunPizzaShop.Domain.Command.Pizza

[<Interface>]
type IAuthentication =
    abstract Login: Login
    abstract Logout: Logout
    abstract Verify: Verify


[<Interface>]
type IPizza =
    abstract Order: OrderPizza


[<Interface>]
type IMailSender =
    abstract SendVerificationMail: SendVerificationMail