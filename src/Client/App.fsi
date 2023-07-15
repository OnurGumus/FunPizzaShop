module FunPizzaShop.Client.App

open Elmish
open Lit
open Lit.Elmish
open Browser
open Elmish.UrlParser
open Elmish.Navigation
open CustomNavigation

type Model = Page option
val init: result: Option<Page> -> Option<Page> * Cmd<'a>
val update: msg: 'a -> model: Model -> Model * Cmd<'b>

[<HookComponent>]
val view: model: Model -> dispatch: 'a -> TemplateResult

val pageParser: Parser<(Page -> Page), Page>
val urlUpdate: result: Option<Page> -> model: Page option -> Page option * Cmd<'a>
