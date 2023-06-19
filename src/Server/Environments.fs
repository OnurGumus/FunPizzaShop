module FunPizzaShop.Server.Environments
open FunPizzaShop
open Microsoft.Extensions.Configuration
open Query


[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) =
    let commandApi =
        Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        Query.API.api config commandApi.ActorApi
        
    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

    
    interface IQuery with
        member _.Query(?filter, ?orderby,?orderbydesc, ?thenby, ?thenbydesc, ?take, ?skip) =
            queryApi.Query(?filter = filter, ?orderby = orderby, ?orderbydesc = orderbydesc, ?thenby = thenby, ?thenbydesc = thenbydesc,  ?take = take, ?skip = skip)

        member _.Subscribe(cb) = queryApi.Subscribe(cb)

    