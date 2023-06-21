module FunPizzaShop.Client.App

open Elmish
open Lit
open Lit.Elmish
open Browser
open Elmish.UrlParser
open Elmish.Navigation

type Page =
    | Home
    | About

type Model = Page option

[<RequireQualifiedAccess>]
module Navigation2 =
    [<Literal>]
    val internal NavigatedEvent: string = "NavigatedEvent"

    /// Modify current location
    val modifyUrl: newUrl: string -> Cmd<'a>
    /// Push new location into history and navigate there
    val newUrl: newUrl: string -> state: obj -> Cmd<'a>
    /// Jump to some point in history (positve=forward, nagative=backward)
    val jump: n: int -> Cmd<'a>

val toPage: (Page -> string)
val init: result: Option<Page> -> Option<Page> * Cmd<'a>
val update: msg: 'a -> model: Model -> Model * Cmd<'b>
val view: model: Model -> dispatch: 'a -> TemplateResult
val pageParser: Parser<(Page -> Page), Page>
val urlUpdate: result: Option<Page> -> model: Page option -> Page option * Cmd<'a>
