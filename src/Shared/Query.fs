module FunPizzaShop.Shared.Query

module Pizza =
    open Model
    open Pizza
    type GetSpecials = unit -> Async<PizzaSpecial list>
    type GetToppings = unit -> Async<Topping option>