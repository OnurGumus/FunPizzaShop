module BestFitBox.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Authentication
open FunPizzaShop.Server.Handlers.Pizza
open Microsoft.AspNetCore.Authentication.Cookies
open FunPizzaShop.Domain.Constants
open Elmish.Bridge
open FunPizzaShop.Domain.API
open Elmish

type ServerMsg =
    | Remote of ClientToServer.Msg
    | SomeMsg
    | ClientDisconnected
    
type Model = NoModel

let init (clientDispatch:Dispatch<ServerToClient.Msg>) () =
    printfn "init!!!!!!!!"
    clientDispatch ServerToClient.ServerConnected
    NoModel, Cmd.none

let update (clientDispatch:Dispatch<ServerToClient.Msg>) (msg:ServerMsg) (model:Model) =
    //hub.SendClientIf (fun x -> x < 3) ServerToClient.ServerConnected
    NoModel, Cmd.none
   
let brideServer =
    Bridge.mkServer endpoint init update
    |> Bridge.withConsoleTrace
    |> Bridge.whenDown ClientDisconnected
    |> Bridge.run Giraffe.server

let webApp (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =

    let viewRoute view = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! lay = (layout ctx (view ctx))
                return! htmlString lay next ctx
            }

    let defaultRoute = viewRoute (Index.view env)
        
    let myOrders = viewRoute (MyOrders.view env)
    
    let trackOrder = viewRoute (TrackOrder.view env)
    let auth = requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
    choose [ 
        
        (authenticationHandler env)
        routeCi "/checkout" >=> defaultRoute 
        routeCi "/" >=> defaultRoute

        routex "^.*OrderPizza.*$"
            >=> auth
            >=>(pizzaHandler env)

        routeCi "/myOrders"
            >=> auth
            >=>(myOrders)

        routex "^.*socket.*$"
            >=> auth
            >=> brideServer

        routeCi "/trackOrder"
            >=> auth
            >=>(trackOrder)
    ]

let webAppWrapper (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    fun (next: HttpFunc) (context: HttpContext) -> task { 
        return! webApp env layout next context
     }
