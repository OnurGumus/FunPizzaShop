module FunPizzaShop.Server.Environments

open FunPizzaShop
open Microsoft.Extensions.Configuration
open Query

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv =
    new: config: IConfiguration -> AppEnv
    interface IConfiguration
    interface IQuery
