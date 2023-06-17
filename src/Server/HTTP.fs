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
        """https://unpkg.com/open-props@1.5.9/open-props.min.css"""
    |]
    |> String.concat " "

let styleSrcElem = 
    [|
        """https://unpkg.com/open-props@1.5.9/open-props.min.css"""
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
            style-src 'self' {styleSrc} ;\
            style-src-elem 'self' {styleSrcElem} ;\
            worker-src 'self';\
            form-action 'self';\
            script-src  'wasm-unsafe-eval';\
            frame-src 'self';\
            require-trusted-types-for 'script';\
            trusted-types default dompurify lit-html;\
            ")
    | _ -> ()
    // require-trusted-types-for 'script';\
   
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
                let headers = context.Context.Response.GetTypedHeaders()
                headers.CacheControl <-
                    Microsoft.Net.Http.Headers.CacheControlHeaderValue(
                        Public = true,
                        MaxAge = TimeSpan.FromDays(1)
                    )
    )
