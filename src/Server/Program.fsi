module FunPizzaShop.Server.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Serilog
open Hocon.Extensions.Configuration
open ThrottlingTroll
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Default
open HTTP
open FunPizzaShop.Shared.Constants
open System.Globalization

type Self = Self
val configBuilder: IConfigurationBuilder
val config: IConfigurationRoot
val errorHandler: ex: Exception -> ctx: HttpContext -> (HttpFunc -> HttpContext -> HttpFuncResult)
val configureCors: builder: CorsPolicyBuilder -> unit

val configureApp:
    app: IApplicationBuilder * appEnv: 'a -> unit
        when 'a :> FunPizzaShop.ServerInterfaces.Query.IQuery
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IAuthentication
        and 'a :> IConfiguration
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IPizza

val configureServices: services: IServiceCollection -> unit
val configureLogging: builder: ILoggingBuilder -> unit

val host:
    appEnv: 'a -> args: string array -> IHost
        when 'a :> FunPizzaShop.ServerInterfaces.Query.IQuery
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IAuthentication
        and 'a :> IConfiguration
        and 'a :> FunPizzaShop.ServerInterfaces.Command.IPizza

[<EntryPoint>]
val main: args: string array -> int
