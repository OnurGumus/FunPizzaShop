module BestFitBox.Server.Handlers.Default

open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views


let webApp (env: #_) (layout: HttpContext -> (int -> string) -> string Async) =
    choose [
        route "/"
        >=> (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! lay = (layout ctx Index.view)
                return! htmlString lay next ctx
            })
    ]


let webAppWrapper (env: #_) (layout: HttpContext -> (int -> string) -> string Async) =
    fun (next: HttpFunc) (context: HttpContext) -> 
        task { return! webApp env layout next context }
