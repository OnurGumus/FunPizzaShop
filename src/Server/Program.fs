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
open Microsoft.AspNetCore.StaticFiles
open System.Threading.Tasks
open Hocon.Extensions.Configuration
open Serilog.Context
open ThrottlingTroll
open FunPizzaShop.Server.Views
open BestFitBox.Server.Handlers.Default

bootstrapLogger()
        
type Self = Self
 
let configBuilder =
    ConfigurationBuilder()
        .AddUserSecrets<Self>()
        .AddHoconFile("config.hocon")
        // .AddHoconFile("secrets.hocon", true)
        .AddEnvironmentVariables()

let config = configBuilder.Build()

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
    let app = if isDevelopment then app else app.UseResponseCompression()

    app
        .UseDefaultFiles()
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

    let sConfig = Serilog.configure errorHandler 
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
        .UseThrottlingTroll(Throttling.setOptions)
        .UseGiraffe(handler)

    if env.IsDevelopment() then
        app.UseSpa(fun spa ->
            let path = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "../../.")
            spa.Options.SourcePath <- path
            spa.Options.DevServerPort <- 5173
            spa.UseReactDevelopmentServer(npmScript = "watch"))

        app.UseSerilogRequestLogging() |> ignore

let configureServices (services: IServiceCollection) =
    services
        .AddAuthorization()
        .AddResponseCompression(fun options -> options.EnableForHttps <- true)
        .AddCors()
        .AddGiraffe()
        .AddAntiforgery()
        .AddApplicationInsightsTelemetry()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(
            CookieAuthenticationDefaults.AuthenticationScheme,
            fun options ->
                options.SlidingExpiration <- true
                options.ExpireTimeSpan <- TimeSpan.FromDays(7)
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
        .UseSerilog(Serilog.configureMiddleware)
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
    let mutable ret = 0
    try
        try
            (host args).Run()
        with ex -> 
            Log.Fatal(ex, "Host terminated unexpectedly")
            ret <- -1
    finally
        Log.CloseAndFlush()
    ret
