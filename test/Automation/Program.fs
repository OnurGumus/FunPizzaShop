module FunPizzaShop.Automation.Program

open System.Reflection
open TickSpec

[<AfterScenarioAttribute>]
let quitBrowser () = ()

[<EntryPointAttribute>]
let main _ =
    try
        do
            let ass = Assembly.GetExecutingAssembly()
            let definitions = StepDefinitions(ass)

            [ "Login" ]
            |> Seq.iter (fun source ->
                let s = ass.GetManifestResourceStream("Automation." + source + ".feature")
                definitions.Execute(source, s))
        0
    with e ->
        printf "%A" e
        -1
