module FunPizzaShop.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Authentication
open FunPizzaShop.Server.Handlers.Pizza
open Microsoft.AspNetCore.Authentication.Cookies
open FunPizzaShop.Shared.API

let webApp (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =

    let viewRoute view = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! lay = (layout ctx (view ctx))
                return! htmlString lay next ctx
            }

    let defaultRoute = viewRoute (Index.view env)
        
    let myOrders = viewRoute (MyOrders.view env)
    
    let trackOrder orderId = 
        viewRoute (TrackOrder.view env orderId)
    let auth = requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
    choose [ 
        (authenticationHandler env)
        routeCi "/checkout" >=> defaultRoute 
        routeCi "/" >=> defaultRoute

        routex "^.*OrderPizza.*$"
            >=> auth
            >=>(pizzaHandler env)

        routeCi "/myOrders"
            >=>(myOrders)

        routex "^.*socket.*$"
            >=> auth
            >=>  TrackOrder.brideServer env
        routeCif "/trackOrder/%s"  (fun orderId -> auth >=>(trackOrder orderId))
    ]

let webAppWrapper (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    fun (next: HttpFunc) (context: HttpContext) -> task { 
        return! webApp env layout next context
     }
