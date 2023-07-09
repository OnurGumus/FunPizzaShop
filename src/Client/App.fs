module FunPizzaShop.Client.App

open Elmish
open Lit
open Lit.Elmish
open Browser
open Elmish.UrlParser
open Elmish.Navigation
open CustomNavigation

PizzaMenu.register ()
PizzaItem.register ()
Sidebar.register ()
Checkout.register ()
SignIn.register ()
TrackOrder.register ()

type Model = Page option

let init (result: Option<Page>) = result, Cmd.none //CustomNavigation.newUrl (toPage Home) 1

let update msg (model: Model) = model, Cmd.none

[<HookComponent>]
let view (model: Model) dispatch =
    Hook.useEffectOnChange (model, fun model ->
        let nonCheckout = document.querySelectorAll "main > *:not(fps-checkout)"
        match model with
        | Some Checkout -> 
            for i = 0 to nonCheckout.length - 1 do
                nonCheckout.item(i).toggleAttribute("hidden", true) |> ignore
        | _ -> 
            for i = 0 to nonCheckout.length - 1 do
                nonCheckout.item(i).toggleAttribute("hidden", false) |> ignore
    )
    match model with
    | Some page ->
        match page with
        | Home -> Lit.nothing
        | Checkout -> 
            html $"""
             <fps-checkout></fps-checkout>
            """
    | None -> Lit.nothing

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Home (s "")
        map Home (s "/")
        map Checkout (s "checkout")
    ]

let urlUpdate (result: Option<Page>) model =
    printfn "urlUpdate %A" result
    match result with
    | None -> model, Cmd.none
    | Some page -> Some page, Cmd.none


Program.mkProgram init (update) view
|> Program.withLitOnElement (document.querySelector "main")
|> Program.withConsoleTrace
|> Program.toNavigable (parsePath pageParser) urlUpdate
|> Program.run
