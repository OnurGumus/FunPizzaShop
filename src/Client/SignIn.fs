module FunPizzaShop.Client.SignIn

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core
open System
open Browser
open Elmish.Debug
open FsToolkit.ErrorHandling
open ElmishOrder
open Browser.Types
open FunPizzaShop.MVU.SignIn
open FunPizzaShop.Domain.Model
open Pizza
open FunPizzaShop.Domain.Constants

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    match model.Status with
    | LoggedIn user -> Lit.nothing
    | VerificationSent -> Lit.nothing
    | NotLoggedIn -> 
        html $"""
            <div class=user-info>
                <a class=sign-in @click={Ev(fun _ -> dispatch LoginRequested )}>Sign In</a>
            </div>
        """
    | Status.LoginRequested ->
        html $"""
            <div class="dialog-container">
                <div class="dialog">
                    <div class="dialog-title">
                        <h2>Sign In</h2>
                    </div>
                    <form class="dialog-body">
                        $FormItems
                    </form>

                    <div class="dialog-buttons">
                        <button id="cancelButton" @click={Ev(fun _ -> dispatch LoginCancelled )} class="btn btn-secondary mr-auto" >Cancel</button>
                        <button id="confirmButton" class="btn btn-success ml-auto">Sign in ></button>
                    </div>
                </div>
            </div>
        """

[<LitElement("fps-signin")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, prop = LitElement.init (fun config -> 
        config.useShadowDom <- false
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute 
            (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()
