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
open BestFitBox.Server.Handlers.Default
open HTTP
open FunPizzaShop.Domain.Constants

type Self = Self
val configBuilder: IConfigurationBuilder
val config: IConfigurationRoot
val errorHandler: ex: Exception -> ctx: HttpContext -> (HttpFunc -> HttpContext -> HttpFuncResult)
val configureCors: builder: CorsPolicyBuilder -> unit
val configureApp: app: IApplicationBuilder -> unit
val configureServices: services: IServiceCollection -> unit
val configureLogging: builder: ILoggingBuilder -> unit
val host: args: string array -> IHost

[<EntryPoint>]
val main: args: string array -> int
