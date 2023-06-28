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
open FunPizzaShop.MVU
open SignIn
open FunPizzaShop.Domain.Model
open Pizza
open FunPizzaShop.Domain.Constants
open Authentication
let private hmr = HMR.createToken ()


module Server =
    open Fable.Remoting.Client
    open FunPizzaShop.Domain
    let api: API.Authentication =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder API.Route.builder
        |> Remoting.buildProxy<API.Authentication>

let rec execute (host :LitElement) order dispatch =
    match order with
    | (Order.PublishLogin email) ->

        LoginStore.dispatcher (LoginStore.Msg.LoggedIn (email))
    | Order.ShowError ex ->
        window.alert(ex)
    | Order.Login email ->
        async {
            let! result = Server.api.Login email
            match result with
            | Ok _ -> dispatch  EmailSent
            | Error e -> dispatch (EmailFailed (sprintf "%A" e))
        }
        |> Async.StartImmediate

    | Order.Verify (email,code) ->
        async {
            try
                let! result = Server.api.Verify (email, Some code)
                match result with
                | Ok _ -> 
                    dispatch VerificationSuccessful
                    LoginStore.dispatcher (LoginStore.Msg.LoggedIn (email))
                | Error e -> dispatch (VerificationFailed)
            with :?  Fable.Remoting.Client.ProxyRequestException as ex ->
                window.alert(ex.ResponseText)
        }
        |> Async.StartImmediate

    | Order.Logout (email) ->
        async {
            let! result = Server.api.Logout email
            match result with
            | Ok _ -> 
                dispatch LogoutSuccess
            | Error e -> 
                let errorText = sprintf "%A" e
                let msg = Msg.LogoutError errorText
                dispatch msg
        }
        |> Async.StartImmediate
    | Order.NoOrder -> ()

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    Hook.useEffectOnce (fun () ->
    let requestLogin (e: Event) =
        dispatch (LoginRequested)
        
    document.addEventListener (Events.RequestLogin, requestLogin) |> ignore

    Hook.createDisposable (fun () -> document.removeEventListener (Events.RequestLogin, requestLogin)))
    let emailField = 
        match model.Status with
        | AskEmail -> 
            html $"""
            <div class="form-field">
                <label>Email:</label>
                <div>
                    <input name=email placeholder="Email" type=email required  />
                </div>
            </div>
            """
        | AskVerification -> 
            html $"""
            <div class="form-field">
                <label>Verification:</label>
                <div>
                    <input name=verification placeholder="Verification" type=text required  />
                </div>
            </div>
            """
        | _ -> Lit.nothing

    match model.Status with
    | LoggedIn user -> 
        html $"""
         <div class=user-info>
            <img src="/assets/user.svg" class="user-icon" />
            <div>
                <span class=username>{user.Value}</span>
                <a class=sign-out
                @click={Ev(fun _ -> dispatch (LogoutRequested))}>Sign Out</a>
            </div>
        </div>
        """
    
    | NotLoggedIn -> 
        html $"""
            <div class=user-info>
                <a class=sign-in @click={Ev(fun _ -> dispatch LoginRequested )}>Sign In</a>
            </div>
        """
    | AskVerification
    | AskEmail ->
        let onsubmit (e: Event) =
            e.preventDefault() |> ignore
            let form = e.target :?> HTMLFormElement
            match model.Status with
            | AskEmail -> 
                let email = form?email?value
                let userId = (UserId.TryCreate email) |> forceValidate
                dispatch (EmailSubmitted userId)
            | AskVerification ->
                let verification = form?verification?value
                let userId = (VerificationCode.TryCreate verification) |> forceValidate
                dispatch (VerificationSubmitted userId)
            | _ -> ()

        html $"""
            <div class="dialog-container">
                <div class="dialog">
                    <div class="dialog-title">
                        <h2>Sign In</h2>
                    </div>
                    <form id=myform class="dialog-body" @submit={Ev(fun e -> onsubmit(e))}>
                        { emailField }
                    </form>

                    <div class="dialog-buttons">
                        <button id="cancelButton" @click={Ev(fun _ -> dispatch LoginCancelled )} class="btn btn-secondary mr-auto" >Cancel</button>
                        <button id="confirmButton"  type="submit" form="myform" class="btn btn-success ml-auto">Sign in ></button>
                    </div>
                </div>
            </div>
        """

[<LitElement("fps-signin")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, prop = LitElement.init (fun config -> 
        config.useShadowDom <- false
        config.props <-
        {|
            username = Prop.Of( Option.None , attribute="username")
        |}
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute 
            (init prop.username.Value ) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()
