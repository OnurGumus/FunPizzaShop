module FunPizzaShop.Domain.API
open Command
open Authentication

type Authentication = {
    Login: Login
    Verify: Verify
    Logout: Logout
}


module Client =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName = sprintf "http://localhost:8000/api/%s/%s" typeName methodName
    
module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName