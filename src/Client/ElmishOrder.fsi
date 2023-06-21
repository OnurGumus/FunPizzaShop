module ElmishOrder

open Elmish
open Lit.Elmish

val effect: dispatching: (ElmishStore.Dispatch<'msg> -> unit) -> ElmishStore.Cmd<'value, 'msg>

module Program =
    /// <summary>
    /// Program with user-defined orders instead of usual command.
    /// Orders are processed by <code>execute</code> which can dispatch messages,
    /// called in place of usual command processing.
    /// </summary>
    val mkHiddenProgramWithOrderExecute:
        init: ('arg' -> 'model * 'order) ->
        update: ('msg -> 'model -> 'model * 'order) ->
        execute: ('order -> Dispatch<'msg> -> unit) ->
            Program<'arg', 'model, 'msg, unit>

    val mkStoreWithOrderExecute:
        init: ('arg' -> 'model * 'order) ->
        update: ('msg -> 'model -> 'model * 'order) ->
        dispose: ('model -> unit) ->
        execute: ('order -> ElmishStore.Dispatch<'msg> -> unit) ->
            ('arg' -> Fable.IStore<'model> * ElmishStore.Dispatch<'msg>)
