module FunPizzaShop.Automation.LoginLogout
open FunPizzaShop
open Microsoft.Playwright
open TickSpec
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open type Microsoft.Playwright.Assertions
open System.IO
open System.Diagnostics
open FunPizzaShop.Server
open FunPizzaShop.ServerInterfaces.Command
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Model
open System
open Microsoft.Extensions.Configuration
open Hocon.Extensions.Configuration
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Automation.Setup

[<BeforeScenario>]
let setUpContext () = 
    (task {
        let! context = browser.NewContextAsync(BrowserNewContextOptions(IgnoreHTTPSErrors = true))
        return (context,sr)
    }).Result

[<AfterScenario>]
let afterContext () = 
     appEnv.Reset()

[<Given>]
let ``I was asked for login`` (context:IBrowserContext, sr: SendVerificationMail ref) =
    (task{
        let verificationCode: VerificationCode ref = ref Unchecked.defaultof<_>

        let trimNonNumeric (s:string) =
            s.ToCharArray() |> Array.filter(Char.IsDigit) |> String
          // failwith "not implemented"
        sr.Value <- fun _ _ code ->
          verificationCode.Value <- 
              code.Value
              |> trimNonNumeric 
              |> VerificationCode.TryCreate |> forceValidate
          async { return () }
       
        do! context.ClearCookiesAsync()
       
       
        do! Task.Delay 2000
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        do! page.GetByText("SIGN IN").ClickAsync()
        return (page, verificationCode)
    }).Result
    


[<When>]
let ``I provided a valid email address`` (page:IPage, verificationCode:VerificationCode ref) = 
    printfn "when"
    (task{
        let email = page.GetByPlaceholder("Email")
        do! email.FillAsync("onur@outlook.com.tr")
        let! button =  page.QuerySelectorAsync("#confirmButton")
        do! button.ClickAsync()
        do! Task.Delay 3000
        printfn "verification code: %A" verificationCode
        return (page)
    }).Result


[<Then>]
let ``it should ask me verification code`` (page:IPage)= 
    (task{
        let verification = page.GetByPlaceholder("Verification").First
        do! Expect(verification).ToBeVisibleAsync()
    }).Wait()