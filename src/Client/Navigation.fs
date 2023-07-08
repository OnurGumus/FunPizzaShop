module CustomNavigation

open Browser
open Elmish

[<Literal>]
let internal NavigatedEvent = "NavigatedEvent"

/// Modify current location
let modifyUrl (newUrl: string)  (state: obj) : Cmd<_> = [
    fun _ -> history.replaceState (state, "", newUrl)
]

/// Push new location into history and navigate there
let newUrl (newUrl: string) (state: obj)  =
    history.pushState (state, "", newUrl)
    let ev = CustomEvent.Create(NavigatedEvent)
    window.dispatchEvent ev |> ignore

/// Jump to some point in history (positve=forward, nagative=backward)
let jump (n: int) : Cmd<_> = [
    fun _ -> history.go n
]

type Page =
    | Home
    | Checkout
    
let toPage =
    function
    | Home -> ""
    | Checkout -> "checkout"