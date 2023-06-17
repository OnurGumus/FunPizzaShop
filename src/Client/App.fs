module FunPizzaShop.Client.App

open Elmish
open Lit
open Lit.Elmish
open Browser
open Elmish.UrlParser
open Elmish.Navigation

PizzaMenu.register ()

type Page =
    | Home
    | About

type Model = Page option

let toPage =
    function
    | Home -> "home"
    | About -> "about"

let init (result: Option<Page>) = result, Navigation.newUrl (toPage Home)

let update msg (model: Model) = model, Cmd.none

let view (model: Model) dispatch =
    console.log model
    Lit.nothing

let pageParser: Parser<Page -> Page, Page> =
    oneOf [
        map Home (s "")
        map Home (s "/")
        map Home (s "home")
        map About (s "about")
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
