module FunPizzaShop.Server.Views.Layout

let inline private html (s: string) = s

open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
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

let view (ctx:HttpContext) (env:#_) (isDev) (body: int -> string) = async{
    let script =
        if isDev || path.Length = 0 then
            html
                $"""
                <script type="module" src="/dist/@vite/client"></script>
                <script type="module" src="/dist/build/App.js"></script>
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
                
    return
        html $"""
    <!DOCTYPE html>
    <html theme=default lang="en">
        <head>
            <meta charset="utf-8" >
            <base href="/" />
            <title>Fun Pizza Shop </title>

            <meta name="description"
                content="Best Pizza in the Town" />
            <meta name="keywords" content="Order Pizza">

            <link rel="apple-touch-icon" href="/assets/icons/favicon-rounded.png">
            <!-- This meta viewport ensures the webpage's dimensions change according to the device it's on. This is called Responsive Web Design.-->
            <meta name="viewport"
                content="viewport-fit=cover, width=device-width, initial-scale=1.0" />

            <!-- These meta tags are Apple-specific, and set the web application to run in full-screen mode with a black status bar. Learn more at https://developer.apple.com/library/archive/documentation/AppleApplications/Reference/SafariHTMLRef/Articles/MetaTags.html-->
            <meta name="apple-mobile-web-app-capable" content="yes" />
            <meta name="apple-mobile-web-app-title" content="Fun Pizza Shop" />
            <meta name="apple-mobile-web-app-status-bar-style" content="black" />

            <!-- Imports an icon to represent the document. -->
            <link rel="icon" href="/assets/icons/favicon.png" type="image/x-icon" />

            <!-- Imports the manifest to represent the web application. A web app must have a manifest to be a PWA. -->
            <link rel="manifest" href="/manifest.webmanifest" />
            <link rel="stylesheet" href="index.css"/>
            <script defer crossorigin="anonymous" type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/dompurify/3.0.1/purify.min.js"></script>
        
        </head>
        <script nonce="110888888">
                if (typeof trustedTypes !== "undefined") {{
                    trustedTypes.createPolicy('default', {{
                        createHTML: (string, sink) => DOMPurify.sanitize(string, {{RETURN_TRUSTED_TYPE: true}})
                    }});
                    const policy = trustedTypes.createPolicy('myPolicy', {{ createScriptURL: (s) => s }});
                    const url = policy.createScriptURL('./sw.js');
                    if ("serviceWorker" in navigator) {{
                    navigator.serviceWorker.register(url);
                    }}
                }}
                else{{
                    if ("serviceWorker" in navigator) {{
                        navigator.serviceWorker.register('./sw.js');
                    }}
                }}
        </script>
        <body>
            <main>
                {body 0}
            </main>
            {script}
            <script nonce="110888888" defer>
                function attachShadowRoots(root) {{
                    root.querySelectorAll("template[shadowrootmode]").forEach(template => {{
                        let shadowRoot = root.shadowRoot
                        if(root.shadowRoot==null){{
                            const mode = template.getAttribute("shadowrootmode");
                            shadowRoot = template.parentNode.attachShadow({{ mode }});
                        }}
                        shadowRoot.appendChild(template.content);
                        template.remove();
                        attachShadowRoots(shadowRoot);
                        
                    }})
                }}
                attachShadowRoots(document.body);
            </script>
            <script src="/_framework/aspnetcore-browser-refresh.js"></script>
        </body>
    </html>"""
    }