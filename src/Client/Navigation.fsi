module CustomNavigation

open Browser
open Elmish

[<Literal>]
val internal NavigatedEvent: string = "NavigatedEvent"

/// Modify current location
val modifyUrl: newUrl: string -> state: obj -> Cmd<'a>
/// Push new location into history and navigate there
val newUrl: newUrl: string -> state: obj -> unit
/// Jump to some point in history (positve=forward, nagative=backward)
val jump: n: int -> Cmd<'a>

type Page =
    | Home
    | Checkout

val toPage: Page -> string
