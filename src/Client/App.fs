module FunPizzaShop.Client.App

open Elmish
open Lit
open Lit.Elmish
open Browser
open Elmish.UrlParser
open Elmish.Navigation

PizzaMenu.register ()
PizzaItem.register ()
Sidebar.register ()

type Page =
    | Home
    | Checkout

type Model = Page option

let toPage =
    function
    | Home -> ""
    | Checkout -> "checkout"

let init (result: Option<Page>) = result, Cmd.none //CustomNavigation.newUrl (toPage Home) 1

let update msg (model: Model) = model, Cmd.none

let view (model: Model) dispatch =
    Lit.nothing

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Home (s "")
        map Home (s "/")
        map Checkout (s "checkout")
    ]

let urlUpdate (result: Option<Page>) model =
    match result with
    | None -> model, Cmd.none
    | Some page -> Some page, Cmd.none


Program.mkProgram init (update) view
|> Program.withLitOnElement (document.querySelector "main")
|> Program.withConsoleTrace
|> Program.toNavigable (parsePath pageParser) urlUpdate
|> Program.run
