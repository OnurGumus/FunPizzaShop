module FunPizzaShop.Server.Views.Layout

let inline private html (s: string) = s

open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
open System.Threading.Tasks
open Common
let scriptFiles =
    let assetsDir = "WebRoot/dist/assets"

    if Directory.Exists assetsDir then
        Directory.GetFiles(assetsDir, "*.js", SearchOption.AllDirectories)
    else
        [||]

let path =
    scriptFiles
    |> Array.map (fun x -> x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1))

let view (ctx:HttpContext) (env:_) (isDev) (body: int -> Task<string>) = task{
    let script =
        if isDev || path.Length = 0 then
            html
                $"""
                <script type="module" src="/dist/@vite/client"></script>
                <script type="module" src="/dist/build/App.js"></script>
                <script defer src="/_framework/aspnetcore-browser-refresh.js"></script>
            """
        else
            let scripts =
                path
                |> Array.map (fun path ->
                    html
                        $"""
                    <script type="module" src="/dist/assets/{path}" ></script>
                    """)
            String.Join("\r\n", scripts)
            
    let! body = body 0
                
    let signin = 
        if ctx.User.Identity.IsAuthenticated then
            html $"""<fps-signin username={ ctx.User.Identity.Name }></fps-signin>"""
        else
            html $"""<fps-signin></fps-signin>"""

    let isMyOrders = 
        ctx.Request.Path.Value.Contains("myOrders")   
    let isRoot = 
        ctx.Request.Path.Value = "/"

    return
        html $"""
    <!DOCTYPE html>
    <html theme=default-pizza lang="en">
        <head>
            <meta charset="utf-8" >
            <base href="/" />
            <title>Fun Pizza Shop </title>

            <meta name="description"
                content="Best Pizza in the Town" />
            <meta name="keywords" content="Order Pizza">

            <link rel="apple-touch-icon" href="/assets/icons/icon-512.png">
            <!-- This meta viewport ensures the webpage's dimensions change according to the device it's on. This is called Responsive Web Design.-->
            <meta name="viewport"
                content="viewport-fit=cover, width=device-width, initial-scale=1.0" />
            <meta name="theme-color"  content="#181818" />

            <!-- These meta tags are Apple-specific, and set the web application to run in full-screen mode with a black status bar. Learn more at https://developer.apple.com/library/archive/documentation/AppleApplications/Reference/SafariHTMLRef/Articles/MetaTags.html-->
            <meta name="apple-mobile-web-app-capable" content="yes" />
            <meta name="apple-mobile-web-app-title" content="Fun Pizza Shop" />
            <meta name="apple-mobile-web-app-status-bar-style" content="black" />

            <!-- Imports an icon to represent the document. -->
            <link rel="icon" href="/assets/icons/icon-512.svg" type="image/x-icon" />

            <!-- Imports the manifest to represent the web application. A web app must have a manifest to be a PWA. -->
            <link rel="manifest" href="/manifest.webmanifest" />
            <link rel="stylesheet" href="/css/index.css?v=202307101701"/>
            <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
     integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="
     crossorigin=""/>
            
            <script defer crossorigin="anonymous" type="text/javascript" 
            src="https://cdnjs.cloudflare.com/ajax/libs/dompurify/3.0.1/purify.min.js"></script>
            <script defer src="/index.js"></script>
            <script defer src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
     integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="
     crossorigin=""></script>
            {script}

        </head>
       
        <body>
            <header>
                <img class=logo src = "/assets/icons/logo.svg" alt="Fun Pizza Shop"/>
                    <a href="/" class='nav-tab { if isRoot then "active" else ""}'>
                        <img src="/assets/icons/pizza-slice.svg" alt="Get Pizza" />
                        <div>Get Pizza</div>
                    </a>
                    <a href="/myOrders" class='nav-tab { if isMyOrders then "active" else ""}'>
                        <img src="/assets/icons/bike.svg" alt="My Orders" />
                        <span>My Orders</span>
                    </a>
                    {signin}
            </header>
            <main>
                {body}
            </main>
        </body>
    </html>"""
    }