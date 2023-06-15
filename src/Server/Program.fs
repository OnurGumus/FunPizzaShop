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
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Serilog
open Serilog.Sinks.SystemConsole
open Serilog.Sinks.File
open Microsoft.ApplicationInsights.Extensibility
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Thoth.Json.Net
open System
open System.Diagnostics
open Microsoft.Extensions.Configuration
open Serilog.Sinks.Async
open Microsoft.AspNetCore.StaticFiles
open System
open System.Threading.Tasks
open Fable.Remoting.Giraffe
open System.Net
open Hocon.Extensions.Configuration
open Stripe
open Serilog.Events
open Serilog.Context
open Serilog.Formatting.Compact
open Serilog.Filters
open ThrottlingTroll
open FunPizzaShop.Server.Views
open BestFitBox.Server.Handlers.Default

type Self = Self

let configBuilder =
    ConfigurationBuilder()
        .AddUserSecrets<Self>()
        // .AddHoconFile("config.hocon", true)
        // .AddHoconFile("secrets.hocon", true)
        .AddEnvironmentVariables()
// .AddInMemoryCollection(clientPath)

let config = configBuilder.Build()

Log.Logger <-
    LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(
            new CompactJsonFormatter(),
            "logs/log_boot_strapper_json_.txt",
            rollingInterval = RollingInterval.Day
        )
        .CreateBootstrapLogger()

type LogUserNameMiddleware(next: RequestDelegate) =
    member private this.next = next

    member this.Invoke(context: HttpContext) : Task =
        LogContext.PushProperty("UserName", context.User.Identity.Name) |> ignore
        this.next.Invoke context

let errorHandler (ex: Exception) (ctx: HttpContext) =
    match ex with
    | :? System.Text.Json.JsonException -> clearResponse >=> setStatusCode 400 >=> text ex.Message
    | _ -> clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:8000", "https://localhost:8001")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let srciptSrcElem =
    [|
       
        """data:"""
        """'nonce-110888888'"""
        """https://cdnjs.cloudflare.com/ajax/libs/dompurify/"""
    |]
    |> String.concat " "

let styleSrcWithHashes =
    [|
        """'nonce-110888888'"""
    |]
    |> String.concat " "

let styleSrc =
    [|
    |]
    |> String.concat " "
    
let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    let isDevelopment = env.IsDevelopment()
    let config = app.ApplicationServices.GetService<IConfiguration>()
    let appEnv = Environments.AppEnv(config)
    let provider = FileExtensionContentTypeProvider()

    provider.Mappings[".css"] <- "text/css; charset=utf-8"
    provider.Mappings[".js"] <- "text/javascript; charset=utf-8"
    provider.Mappings[".webmanifest"] <- "application/manifest+json; charset=utf-8"
    let app = app.UseDefaultFiles()
    let app = if isDevelopment then app else app.UseResponseCompression()

    app
        .UseAuthentication()
        .UseAuthorization()
        .UseMiddleware<LogUserNameMiddleware>()
        .Use(fun (context: HttpContext) (next: Func<Task>) ->
            let headers = context.Response.Headers
            headers.Add("X-Content-Type-Options", "nosniff")

            match context.Request.Headers.TryGetValue("Accept") with
            | true, accept ->
                if accept |> Seq.exists (fun x -> x.Contains "text/html") then
                    headers.Add("Cross-Origin-Embedder-Policy", "corp")
                    headers.Add("Cross-Origin-Opener-Policy", "same-origin")

                headers.Add(
                    "Content-Security-Policy",
                    $"default-src 'none';\
                    font-src 'self';\
                    img-src 'self';\
                    manifest-src 'self';\
                    script-src-elem 'self' {srciptSrcElem} ;\
                    connect-src 'self'  localhost ws://192.168.50.236:* ws://localhost:* http://localhost:*/dist/ https://localhost:*/dist/;\
                    style-src 'self' {styleSrc} ;\
                    worker-src 'self';\
                    form-action 'self';\
                    script-src  'wasm-unsafe-eval';\
                    frame-src 'self';\
                    ")
            | _ -> ()

            next.Invoke())
    |> ignore

    let layout ctx = Layout.view ctx (appEnv) (env.IsDevelopment())

    let sConfig = {
        SerilogConfig.defaults with
            ErrorHandler = errorHandler
            RequestMessageTemplate = "{Method} Request at {Path}, User: {UserName}"
            ResponseMessageTemplate =
                "{Method} Response (StatusCode {StatusCode}) at {Path} took {Duration} ms, User: {UserName}"
    }
    let handler = SerilogAdapter.Enable(webAppWrapper appEnv layout, sConfig)

    (match isDevelopment with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles(
            StaticFileOptions(
                ContentTypeProvider = provider,
                OnPrepareResponse =
                    fun (context) ->
                        let headers = context.Context.Response.GetTypedHeaders()

                        headers.CacheControl <-
                            Microsoft.Net.Http.Headers.CacheControlHeaderValue(
                                Public = true,
                                MaxAge = TimeSpan.FromDays(1)
                            )
            )
        )
        .UseThrottlingTroll(fun options ->
            let config = ThrottlingTrollConfig()
            config.Rules <- [|
                ThrottlingTrollRule(
                    UriPattern = "/api/Authentication/Login",
                    LimitMethod =
                        FixedWindowRateLimitMethod(PermitLimit = 5, IntervalInSeconds = 60),
                    IdentityIdExtractor = fun (request) -> 
                            let r = (request :?>IIncomingHttpRequestProxy).Request
                            r.HttpContext.Connection.RemoteIpAddress.ToString()
                )
                ThrottlingTrollRule(
                    UriPattern = "/api/Authentication/Login",
                    LimitMethod =
                        FixedWindowRateLimitMethod(PermitLimit = 7, IntervalInSeconds = 600),
                    IdentityIdExtractor = fun (request) -> 
                            let r = (request :?>IIncomingHttpRequestProxy).Request
                            r.HttpContext.Connection.RemoteIpAddress.ToString()
                )
                ThrottlingTrollRule(
                    UriPattern = "/api/Authentication/Verify",
                    LimitMethod =
                        FixedWindowRateLimitMethod(PermitLimit = 7, IntervalInSeconds = 60),
                    IdentityIdExtractor = fun (request) -> 
                            let r = (request :?>IIncomingHttpRequestProxy).Request
                            r.HttpContext.Connection.RemoteIpAddress.ToString()
                )
                ThrottlingTrollRule(
                    UriPattern = "/api/Authentication/Verify",
                    LimitMethod =
                        FixedWindowRateLimitMethod(PermitLimit = 15, IntervalInSeconds = 600),
                    IdentityIdExtractor = fun (request) -> 
                            let r = (request :?>IIncomingHttpRequestProxy).Request
                            r.HttpContext.Connection.RemoteIpAddress.ToString()
                )
            |]
           

            options.Config <- config)
        .UseGiraffe(handler)

    if env.IsDevelopment() then
        app.UseSpa(fun spa ->
            let path = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "../../.")
            spa.Options.SourcePath <- path
            printfn "path%A" spa.Options.SourcePath
            spa.Options.DevServerPort <- 5173
            spa.UseReactDevelopmentServer(npmScript = "watch"))

        app.UseSerilogRequestLogging() |> ignore

let configureServices (services: IServiceCollection) =
    services.AddAuthorization() |> ignore

    services.AddResponseCompression(fun options -> options.EnableForHttps <- true)
    |> ignore

    services
        .AddCors()
        .AddGiraffe()
        .AddAntiforgery()
        .AddApplicationInsightsTelemetry()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(
            CookieAuthenticationDefaults.AuthenticationScheme,
            fun options ->
                // options.LoginPath <- "/login";
                options.SlidingExpiration <- true
                options.ExpireTimeSpan <- TimeSpan.FromDays(7)
        //   options.AccessDeniedPath <- "/login"
        )
    |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

let host args =
  //  DB.init config
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .UseSerilog(fun context services loggerConfiguration ->

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
                //  .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
                .WriteTo.File(new CompactJsonFormatter(), "logs/log_json_.txt", rollingInterval = RollingInterval.Day)
                .Destructure.FSharpTypes()
                .WriteTo.Console()
#if DEBUG
                .WriteTo.Seq("http://192.168.50.236:5341")
#endif
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(
                    services.GetRequiredService<TelemetryConfiguration>(),
                    TelemetryConverter.Traces)
            |> ignore)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
#if !DEBUG
                .UseEnvironment(Environments.Production)
#else
                .UseEnvironment(Environments.Development)
#endif
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()

[<EntryPoint>]
let main args =
    (host args).Run()
    0
