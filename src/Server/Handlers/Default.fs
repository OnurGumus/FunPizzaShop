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
    |> Bridge.run Giraffe.server

let webApp (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    let defaultRoute =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let view = Index.view env
                let! lay = (layout ctx view)
                return! htmlString lay next ctx
            }
            
    let myOrders =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let view = MyOrders.view ctx env
                let! lay = (layout ctx view)
                return! htmlString lay next ctx
            }
    let trackOrder =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let view = TrackOrder.view ctx env
                let! lay = (layout ctx view)
                return! htmlString lay next ctx
            }

    choose [ 
        brideServer
        (authenticationHandler env)
        routeCi "/checkout" >=> defaultRoute 
        routeCi "/" >=> defaultRoute
        routex "^.*OrderPizza.*$"
            >=>requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
            >=>(pizzaHandler env)
        routex "^.*myOrders.*$"
            >=>requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
            >=> routeCi "/myOrders"
            >=>(myOrders)
        routeCi "/trackOrder"
            >=>requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
            >=>(trackOrder)
        
    ]


let webAppWrapper (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    fun (next: HttpFunc) (context: HttpContext) -> task { 
        return! webApp env layout next context
     }
