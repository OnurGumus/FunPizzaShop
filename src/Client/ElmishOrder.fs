module ElmishOrder
open Elmish
open Lit.Elmish

let effect (dispatching:ElmishStore.Dispatch<'msg> ->unit) : ElmishStore.Cmd<'value, 'msg> =
    [fun _update dispatch -> dispatching dispatch]
module Program =
    /// <summary>
    /// Program with user-defined orders instead of usual command.
    /// Orders are processed by <code>execute</code> which can dispatch messages,
    /// called in place of usual command processing.
    /// </summary>
    let mkHiddenProgramWithOrderExecute
            (init: 'arg' -> 'model * 'order)
            (update: 'msg -> 'model -> 'model * 'order)
            (execute: 'order -> Dispatch<'msg> -> unit) =
        let convert (model, order) = 
            model, order |> execute |> Cmd.ofEffect 
        Program.mkHidden
            (init >> convert)
            (fun msg model -> update msg model |> convert)
            
    let mkStoreWithOrderExecute
            (init: 'arg' -> 'model * 'order)
            (update: 'msg -> 'model -> 'model * 'order)
            (dispose: 'model -> unit)
            (execute: 'order ->ElmishStore.Dispatch<'msg> ->unit) =
        let convert (model, order) = 
            model, order |> execute |>effect
        Store.makeElmish
            (init >> convert)
            (fun msg model -> update msg model |> convert)
            dispose