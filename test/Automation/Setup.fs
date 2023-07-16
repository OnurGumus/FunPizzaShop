module FunPizzaShop.Automation.Setup
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

Directory.SetCurrentDirectory("/workspaces/FunPizzaShop/src/Server")

let sr:SendVerificationMail ref = ref Unchecked.defaultof<_>
let mailSender = 
    { new IMailSender with
        member _.SendVerificationMail =
            sr.Value

    }
let appEnv = new Environments.AppEnv(config, mailSender)
let host = (App.host appEnv [||])
host.Start()
let playwright = Playwright.CreateAsync().Result
let browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true)).Result

