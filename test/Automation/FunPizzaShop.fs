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
let configBuilder =
    ConfigurationBuilder()
        .AddHoconFile("test-config.hocon")
        .AddEnvironmentVariables()

let config = configBuilder.Build()
let mutable host: IHost = Unchecked.defaultof<_>
let appEnv: Environments.AppEnv ref = ref Unchecked.defaultof<_>

[<BeforeScenario>]
let setUpContext () = 
    (task {
        let! playwright = Playwright.CreateAsync()
        let! browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true))
        let! context = browser.NewContextAsync(BrowserNewContextOptions(IgnoreHTTPSErrors = true))
        let sr:SendVerificationMail ref = ref Unchecked.defaultof<_>
        let mailSender = 
            { new IMailSender with
                member _.SendVerificationMail =
                    sr.Value
        
            }
        appEnv.Value <- new Environments.AppEnv(config, mailSender)
        
        return (context, appEnv,sr)
    }).Result

[<AfterScenario>]
let afterContext () = 
    (task {
        if (host <> Unchecked.defaultof<_>) then
            printfn "stopping host"
            (appEnv.Value:IDisposable).Dispose()
            host.StopAsync().Wait()
    }).Result

[<Given>]
let ``I am at login screen`` (context:IBrowserContext, sr: SendVerificationMail ref) =
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
       
        host <- (App.host appEnv.Value [||])
        host.Start()
        do! Task.Delay 2000
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        do! page.GetByText("SIGN IN").ClickAsync()
        return (page, verificationCode)
    }).Result
    


[<When>]
let ``I typed a valid email address`` (page:IPage, verificationCode:VerificationCode ref) = 
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