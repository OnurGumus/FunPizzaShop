module BestFitBox.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Authentication
open FunPizzaShop.Server.Handlers.Pizza
open Microsoft.AspNetCore.Authentication.Cookies


let webApp (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    let defaultt =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let indexView = Index.view env
                let! lay = (layout ctx indexView)
                return! htmlString lay next ctx
            }
    choose [ 
        (authenticationHandler env)
        routeCi "/checkout" >=> defaultt; routeCi "/" >=> defaultt
        routex "^.*OrderPizza.*$"
        >=>requiresAuthentication (challenge CookieAuthenticationDefaults.AuthenticationScheme)
        >=>(pizzaHandler env)
    ]


let webAppWrapper (env: #_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    fun (next: HttpFunc) (context: HttpContext) -> task { 
        return! webApp env layout next context
     }
