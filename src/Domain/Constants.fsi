module FunPizzaShop.Domain.Constants

[<Literal>]
val Akka: string = "config:akka"

[<Literal>]
val ConnectionString: string = "config:connection-string"

[<Literal>]
val Socket_Endpoint: string = "/socket/main"

[<Literal>]
val ConfigHocon: string = "config.hocon"

[<Literal>]
val ClientPath: string = "ClientPath"

module Events =
    [<Literal>]
    val PizzaSelected: string = "pizzaSelected"
