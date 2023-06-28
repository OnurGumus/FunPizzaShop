module FunPizzaShop.Domain.Command

module Authentication = 
    open Model.Authentication

    type Login = UserId -> Async<Result<unit, LoginError list>>
    type Verify = UserId * VerificationCode option -> Async<Result<unit, VerificationError list>>
    type Logout = UserId -> Async<Result<unit, LogoutError list>>
    type SendVerificationMail = Email -> Subject -> Body -> Async<unit>

module Pizza =
    open Model.Pizza

    type OrderPizza = Order -> Async<unit>