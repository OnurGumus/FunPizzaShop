module FunPizzaShop.Shared.API
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

module TrackOrder =
    open Model.Pizza

    // Messages processed on the server
    module ServerToClient =

        type Msg =
            | ServerConnected
            | OrderFound of Order
            | LocationUpdated of OrderId * LatLong
            | DeliveryStatusSet of OrderId * DeliveryStatus

    module ClientToServer =
        type Msg =
            | TrackOrder of OrderId

    let endpoint = "/socket/track-order"


module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName