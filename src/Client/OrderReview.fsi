module FunPizzaShop.Client.OrderReview

open Lit
open FunPizzaShop.Shared.Model.Pizza

val pizzaTemplate: pizza: Pizza -> TemplateResult
val sum: pizzas: Pizza list -> decimal
val summ: pizzas: Pizza list -> TemplateResult
val pizzaList: pizzas: Pizza list -> TemplateResult list
