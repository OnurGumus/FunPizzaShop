module FunPizzaShop.Server.Views.Layout

open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
open System.Threading.Tasks
open Common

val scriptFiles: string array
val path: string array
val view: ctx: HttpContext -> env: #_ -> isDev: bool -> body: (int -> Task<string>) -> Task<string>
