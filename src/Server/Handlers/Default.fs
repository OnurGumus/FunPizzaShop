module BestFitBox.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Authentication
open FunPizzaShop.Server.Handlers.Pizza
open Microsoft.AspNetCore.Authentication.Cookies


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
