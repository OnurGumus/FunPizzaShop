module FunPizzaShop.Client.OrderReview

open Lit
open FunPizzaShop.Shared.Model.Pizza

let pizzaTemplate (pizza: Pizza) =
    let toppingItem (topping: Topping) =
        html $"""<li>+ {topping.Name.Value}</li>"""

    let toppingItems = pizza.Toppings |> List.map toppingItem

    html
        $"""
            <p>
                <strong>
                    {pizza.Size}"
                    {pizza.Special.Name.Value}
                    (£{pizza.Special.FormattedBasePrice})
                </strong>
            </p>
            <ul>
                {toppingItems}
            </ul>
    """

let sum pizzas =
    pizzas |> List.sumBy (fun (p: Pizza) -> p.TotalPrice.Value)

let summ pizzas =
    html
        $"""
        <p>
            <strong>
                Total price:
                £{(sum pizzas).ToString("0.00")}
            </strong>
        </p>
    """

let pizzaList pizzas = pizzas |> List.map (pizzaTemplate)
