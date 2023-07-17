module FunPizzaShop.Automation.PizzaMenu
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

[<When>]
let ``I get the main menu``  (context:IBrowserContext)= 
    (task{
        let! page = context.NewPageAsync()
        let! _ = page.GotoAsync("http://localhost:8000")
        return (page)
    }).Result


[<Then>]
let ``pizza items should be fetched`` (page:IPage)= 
    (task{
        let! pizzaItems = page.QuerySelectorAllAsync("fps-pizza-item")
        if pizzaItems.Count = 0 then
            failwith "expected at least 1 pizza"
    }).Wait()


[<Given>]
let ``pizza items are fetched``  (context:IBrowserContext)= 
    (task{
        let page = ``I get the main menu`` (context)
        ``pizza items should be fetched`` (page)
        return (page)
    }).Result


[<When>]
let ``I choose a pizza``  (page:IPage)= 
    (task{
        let! pizzaItems = page.QuerySelectorAllAsync("fps-pizza-item")
        do! pizzaItems[0].ClickAsync()
        return (page)
    }).Result

[<Then>]
let ``that pizza should be selected for configuration`` (page:IPage)= 
    (task{
        let el=  page.GetByText("BASIC CHEESE PIZZA").First
        Expect(el).ToBeVisibleAsync().Wait()
        return()
    }).Wait()
