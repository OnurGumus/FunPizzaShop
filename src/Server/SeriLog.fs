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

let bootstrapLogger () =
    Log.Logger <-
        LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                new CompactJsonFormatter(),
                "logs/log_boot_strapper_json_.txt",
                rollingInterval = RollingInterval.Day
            )
            .CreateBootstrapLogger()

let configure errorHandler = {
    SerilogConfig.defaults with
        ErrorHandler = errorHandler
        RequestMessageTemplate = "{Method} Request at {Path}, User: {UserName}"
        ResponseMessageTemplate =
            "{Method} Response (StatusCode {StatusCode}) at {Path} took {Duration} ms, User: {UserName}"
}


type LogUserNameMiddleware(next: RequestDelegate) =
    member _.Invoke(context: HttpContext) : Task =
        LogContext.PushProperty("UserName", context.User.Identity.Name) |> ignore
        next.Invoke context

let configureMiddleware _ (services:IServiceProvider) (loggerConfiguration:LoggerConfiguration) =

    loggerConfiguration
    #if DEBUG
        .MinimumLevel
        .Debug()
    #else
        .MinimumLevel
        .Information()
    #endif
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Giraffe", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Filter.ByExcluding(
            Matching.WithProperty<string>(
                "UserAgent",
                (fun p -> String.IsNullOrWhiteSpace p || p = "AlwaysOn")
            )
        )
        .Filter.ByExcluding("@m like '%/dist/%'")
        .Filter.ByExcluding("@m = 'Passing through logger HttpHandler'")
        .Filter.ByExcluding(
            "@m like 'GET Response%' and (@p['UserName'] is null) and @p['Path'] = '/' and @p['StatusCode'] = 200"
        )
        .WriteTo.File(new CompactJsonFormatter(), "logs/log_json_.txt", rollingInterval = RollingInterval.Day)
        .Destructure.FSharpTypes()
        .WriteTo.Console()
    #if DEBUG
        .WriteTo.Seq("http://192.168.50.236:5341")
    #else
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces)
    #endif
    |> ignore