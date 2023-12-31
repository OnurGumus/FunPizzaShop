module FunPizzaShop.Automation.FunPizzaShop
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