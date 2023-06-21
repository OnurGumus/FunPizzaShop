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

let view (ctx:HttpContext) (env:#_) (isDev) (body: int -> Task<string>) = task{
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
            <link rel="stylesheet" href="/css/index.css"/>
            
            <script defer crossorigin="anonymous" type="text/javascript" 
            src="https://cdnjs.cloudflare.com/ajax/libs/dompurify/3.0.1/purify.min.js"></script>
            <script defer src="/index.js"></script>
            {script}

        </head>
       
        <body>
            <header>
                <img class=logo src = "/assets/icons/logo.svg" alt="Fun Pizza Shop"/>
               
                    <!-- <ul>
                        <li><a href="/">Home</a></li>
                        <li><a href="/order">Order</a></li>
                        <li><a href="/about">About</a></li>
                    </ul> -->
                    <a href="/" class=nav-tab>
                        <img src="/assets/icons/pizza-slice.svg" alt="Get Pizza" />
                        <div>Get Pizza</div>
                    </a>
                    <a href="/myOrders" class=nav-tab>
                        <img src="/assets/icons/bike.svg" alt="My Orders" />
                        <span>My Orders</span>
                    </a>
                    <div class=user-info>
                        <a class=sign-in>Sign In</a>
                    </div>
            </header>
            <main>
                {body}
                <div class="sidebar">
                    <fps-side-bar></fps-side-bar>
                </div>

            </main>
        </body>
    </html>"""
    }