module FunPizzaShop.Server.HTTP
open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.StaticFiles

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
        """'sha256-s5lCkoBP6YOkvh/CPFGUTfLYaAKWmn/afZOK/RXey84='"""
        """'sha256-gxL9+aniObPX9WtUTxMAXCSVJgXs9M5d6F9ia5CFia0='"""
        """'sha256-Y2t+UQ8/zrxi0m/Zp6z/zKfXxJwbGS82PmmuJK2MhW8='"""
        """'sha256-RrYK/jynWvPyBbaMxKRQkyELYUIMmD1uSJLn5/T3ci0='"""
        """'sha256-ctWNF7ykaOZFUGZfGChlx3SWTkYQ0vp4PZgG/aAk+oY='""" 
        """'sha256-uPWqoOqJlYRh4vuSeJqL7+v95llQo6xvHZ87qSOUfR8='"""
        """'sha256-aRsWYqZCaVHt8N5HotM+QdQl721qCNtGAH5KpRp19+g='"""
        """'sha256-0fbn1I45Wm0gd77UCbWHVcVY1tcwwo/EfrGEzMR7dN8='"""
        """https://unpkg.com/open-props@1.5.9/open-props.min.css"""
        """https://cdn.jsdelivr.net/npm/bootstrap@4.0.0/dist/css/bootstrap.min.css"""
    |]
    |> String.concat " "

let styleSrcElem = 
    [|
        """https://unpkg.com/open-props@1.5.9/open-props.min.css"""
        """https://cdn.jsdelivr.net/npm/bootstrap@4.0.0/dist/css/bootstrap.min.css"""

    |]
    |> String.concat " "

let headerMiddleware = fun (context: HttpContext) (next: Func<Task>) ->
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
            connect-src 'self' localhost ws://192.168.50.236:* ws://localhost:* http://localhost:*/dist/ https://localhost:*/dist/;\
            style-src 'self' 'unsafe-hashes' {styleSrc} ;\
            style-src-elem 'self' {styleSrcElem} ;\
            worker-src 'self';\
            form-action 'self';\
            script-src  'wasm-unsafe-eval';\
            frame-src 'self';\
            require-trusted-types-for 'script';\
            trusted-types *;\
            ")
    | _ -> ()
   
    next.Invoke()


let provider = FileExtensionContentTypeProvider()

provider.Mappings[".css"] <- "text/css; charset=utf-8"
provider.Mappings[".js"] <- "text/javascript; charset=utf-8"
provider.Mappings[".webmanifest"] <- "application/manifest+json; charset=utf-8"

let staticFileOptions = 
    StaticFileOptions(
        ContentTypeProvider = provider,
        OnPrepareResponse =
            fun (context) ->
        #if !DEBUG
                let headers = context.Context.Response.GetTypedHeaders()
                headers.CacheControl <-
                    Microsoft.Net.Http.Headers.CacheControlHeaderValue(
                        Public = true,
                        MaxAge = TimeSpan.FromDays(1)
                    )
        #else
                ()
        #endif
        
    )
