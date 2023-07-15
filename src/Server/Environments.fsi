module FunPizzaShop.Server.Environments

open FunPizzaShop
open FunPizzaShop.ServerInterfaces
open Microsoft.Extensions.Configuration
open Query
open Command
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza
open System.Net.Mail
open System.Net
open Serilog

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv =
    new: config: IConfiguration -> AppEnv
    interface IMailSender
    interface IAuthentication
    interface IPizza
    interface IConfiguration
    interface IQuery
