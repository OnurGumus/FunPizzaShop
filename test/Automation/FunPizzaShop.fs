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
        do! context.ClearCookiesAsync()
        let appEnv = Environments.AppEnv(App.config)
        let host = (App.host appEnv [||])
        host.Start()
        do! Task.Delay 2000
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        do! page.GetByText("SIGN IN").ClickAsync()
        return (page,host)
    }).Result
    


[<When>]
let ``I typed a valid email address`` (page:IPage,host:IHost) = 
    (task{
        let email = page.GetByPlaceholder("Email")
        do! email.FillAsync("onur@outlook.com.tr")
        let! button =  page.QuerySelectorAsync("#confirmButton")
        do! button.ClickAsync()
        return (page,host)
    }).Result


[<Then>]
let ``it should ask me verification code`` (page:IPage,host:IHost)= 
    (task{
        let verification = page.GetByPlaceholder("Verification").First
        do! Expect(verification).ToBeVisibleAsync()
        do! host.StopAsync()
    }).Wait()