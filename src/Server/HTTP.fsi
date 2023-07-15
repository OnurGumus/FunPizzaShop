module FunPizzaShop.Server.HTTP

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.StaticFiles

val srciptSrcElem: string
val styleSrcWithHashes: string
val styleSrc: string
val styleSrcElem: string
val headerMiddleware: context: HttpContext -> next: Func<Task> -> Task
val provider: FileExtensionContentTypeProvider
val staticFileOptions: StaticFileOptions
