module FunPizzaShop.Domain.API
open Command
open Authentication
open Pizza

type Authentication = {
    Login: Login
    Verify: Verify
    Logout: Logout
}

type Order = {
    OrderPizza: OrderPizza
}


// Messages processed on the server
module ServerToClient =
    type CounterMsg = CounterValue of int

    type Msg =
        | ServerConnected
        | CounterAdded of int
        | CounterMessage of CounterMsg * int

module ClientToServer =
    type CounterMsg =
        | StartCounter
        | StopCounter
    type Msg =
        | AddCounter
        | CounterMessage of CounterMsg * int

let endpoint = "/socket/track-order"


module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName