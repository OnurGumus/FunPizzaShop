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

[<RequireQualifiedAccess>]
module Navigation2 =
    let [<Literal>] internal NavigatedEvent = "NavigatedEvent"

    /// Modify current location
    let modifyUrl (newUrl:string):Cmd<_> =
        [fun _ -> history.replaceState((), "", newUrl)]

    /// Push new location into history and navigate there
    let newUrl (newUrl:string) (state:obj):Cmd<_> =
        [fun _ -> history.pushState((), "", newUrl)
                  let ev = CustomEvent.Create(NavigatedEvent)
                  window.dispatchEvent ev
                  |> ignore ]

    /// Jump to some point in history (positve=forward, nagative=backward)
    let jump (n:int):Cmd<_> =
        [fun _ -> history.go n]

let toPage =
    function
    | Home -> "home"
    | About -> "about"

let init (result: Option<Page>) = result, Navigation2.newUrl (toPage Home) 1

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
