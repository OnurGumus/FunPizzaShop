module FunPizzaShop.Server.Environments
open FunPizzaShop
open FunPizzaShop.ServerInterfaces
open Microsoft.Extensions.Configuration
open Query
open Command
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) as self =
    let commandApi =
        lazy(Command.API.api self NodaTime.SystemClock.Instance)

    let queryApi =
        lazy(Query.API.api config commandApi.Value.ActorApi)
        
    interface IMailSender with
        member _.SendVerificationMail =
            fun (email:Email) (subject: Subject) (body: Body) ->
                async{
                    // MailSender.sendMessage 
                    //     config["config:SmtpUser"]
                    //     config.["config:SmtpPass"] 
                    //         (email.Value) (subject.Value) (body.Value)
                    return ()
                }

    interface IAuthentication with
        member _.Login: Login = 
            commandApi.Value.Login
            
        member _.Logout: Logout = 
            fun (userId: UserId) -> 
                async { 
                    return  Ok()
                }
        member _.Verify: Verify = 
            commandApi.Value.Verify

    interface IPizza with
        member _.Order: OrderPizza = 
            commandApi.Value.OrderPizza

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

    
    interface IQuery with
        member _.Query(?filter, ?orderby,?orderbydesc, ?thenby, ?thenbydesc, ?take, ?skip) =
            queryApi.Value.Query(?filter = filter, ?orderby = orderby, ?orderbydesc = orderbydesc, ?thenby = thenby, ?thenbydesc = thenbydesc,  ?take = take, ?skip = skip)

        member _.Subscribe(cb) = queryApi.Value.Subscribe(cb)

    