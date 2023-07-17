module FunPizzaShop.Automation.Program

open System.Reflection
open TickSpec
open FunPizzaShop.Automation.Setup



[<EntryPointAttribute>]
let main _ =
    try
        try
            do
                let ass = Assembly.GetExecutingAssembly()
                let definitions = StepDefinitions(ass)

                [ "Login"; "PizzaMenu" ]
                |> Seq.iter (fun source ->
                    let s = ass.GetManifestResourceStream("Automation." + source + ".feature")
                    definitions.Execute(source, s))
            0
        with e ->
            printf "%A" e
            -1
    finally
        host.StopAsync().Wait()
