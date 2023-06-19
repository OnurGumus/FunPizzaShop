module FunPizzaShop.Server.Views.Index
open Common
open FunPizzaShop.Server.Query
open FunPizzaShop.Domain.Model.Pizza

let view (env:#_) (dataLevel: int) = task{
    let query = env :> IQuery
    let! pizzas = query.Query<PizzaSpecial> () 
    let li = 
        pizzas 
        |> List.map (fun pizza -> html $"""<li>{pizza.Name}</li>""")
        |> String.concat "\r\n"
    return
        html $""" 
            <fps-pizza-menu>{li}</fps-pizza-menu>
        """
}