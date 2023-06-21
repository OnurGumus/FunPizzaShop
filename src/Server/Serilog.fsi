module FunPizzaShop.Server.Serilog

open Serilog
open Serilog.Formatting.Compact
open Giraffe.SerilogExtensions
open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Serilog.Events
open Serilog.Filters
open Microsoft.AspNetCore.Http
open Serilog.Context
open System.Threading.Tasks

val bootstrapLogger: unit -> unit

val configure:
    errorHandler: (Exception -> HttpContext -> Giraffe.Core.HttpFunc -> HttpContext -> Giraffe.Core.HttpFuncResult) ->
        SerilogConfig

type LogUserNameMiddleware =
    new: next: RequestDelegate -> LogUserNameMiddleware
    member Invoke: context: HttpContext -> Task

val configureMiddleware: 'a -> services: IServiceProvider -> loggerConfiguration: LoggerConfiguration -> unit
