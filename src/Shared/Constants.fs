module FunPizzaShop.Shared.Constants


[<Literal>]
let Akka = "config:akka"

[<Literal>]
let ConnectionString = "config:connection-string"

[<Literal>]
let Socket_Endpoint = "/socket/main"

[<Literal>]
let ConfigHocon = "config.hocon"

[<Literal>]
let ClientPath = "ClientPath"

module Events =
    
    [<Literal>]
    let PizzaSelected = "pizzaSelected"

    [<Literal>]
    let PizzaOrdered = "pizzaOrdered"

    [<Literal>]
    let RequestLogin = "requestLogin"