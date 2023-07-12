module FunPizzaShop.Automation.LoginLogout
open FunPizzaShop
open Microsoft.Playwright
open TickSpec
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open type Microsoft.Playwright.Assertions
open System.IO
open System.Diagnostics

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
    context.ClearCookiesAsync().Wait()


[<When>]
let ``I typed a valid email address`` (context:IBrowserContext) = 
    (task{
        let host = (Server.App.host [||])
        host.Start()
        do! Task.Delay 2000
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        return (page,host)
    }).Result


[<Then>]
let ``it should ask me verification code`` (page:IPage,host:IHost)= 
    (task{
        // let form =
        //     page.GetByRole(AriaRole.Form, PageGetByRoleOptions(Name = "Calculation input form" ))
        // do! Expect(form).ToHaveCountAsync(1)
        // printfn "element found"
            
        do! host.StopAsync()
    }).Wait()