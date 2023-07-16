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

let configBuilder =
    ConfigurationBuilder()
        .AddHoconFile("test-config.hocon")
        .AddEnvironmentVariables()

let config = configBuilder.Build()

[<BeforeScenario>]
let setUpContext () = 
    Directory.SetCurrentDirectory("/workspaces/FunPizzaShop/src/Server")
    (task {
        let! playwright = Playwright.CreateAsync()
        let! browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true))
        let! context = browser.NewContextAsync(BrowserNewContextOptions(IgnoreHTTPSErrors = true))
        return context
    }).Result

[<Given>]
let ``I am at login screen`` (context:IBrowserContext) =
    (task{
        let verificationCode: VerificationCode ref = ref Unchecked.defaultof<_>

        let trimNonNumeric (s:string) =
            s.ToCharArray() |> Array.filter(Char.IsDigit) |> String

        let mailSender = 
            { new IMailSender with
                member _.SendVerificationMail =
                    fun _ _ code ->
                    verificationCode.Value <- 
                        code.Value
                        |> trimNonNumeric 
                        |> VerificationCode.TryCreate |> forceValidate
                    async { return () }
            }
        do! context.ClearCookiesAsync()
        let appEnv = Environments.AppEnv(config, mailSender)
        let host = (App.host appEnv [||])
        host.Start()
        do! Task.Delay 2000
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        do! page.GetByText("SIGN IN").ClickAsync()
        return (page,host, verificationCode)
    }).Result
    


[<When>]
let ``I typed a valid email address`` (page:IPage,host:IHost, verificationCode:VerificationCode ref) = 
    (task{
        let email = page.GetByPlaceholder("Email")
        do! email.FillAsync("onur@outlook.com.tr")
        let! button =  page.QuerySelectorAsync("#confirmButton")
        do! button.ClickAsync()
        do! Task.Delay 3000
        printfn "verification code: %A" verificationCode
        return (page,host)
    }).Result


[<Then>]
let ``it should ask me verification code`` (page:IPage,host:IHost)= 
    (task{
        let verification = page.GetByPlaceholder("Verification").First
        do! Expect(verification).ToBeVisibleAsync()
        do! host.StopAsync()
    }).Wait()