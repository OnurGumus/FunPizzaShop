module FunPizzaShop.Server.Environments
open FunPizzaShop
open Microsoft.Extensions.Configuration
open Query
open Command
open FunPizzaShop.Domain.Model.Authentication
open FunPizzaShop.Domain.Command.Authentication


[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) =
    let commandApi =
        Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        Query.API.api config commandApi.ActorApi
        
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
            commandApi.Login
            
        member _.Logout: Logout = 
            fun (userId: UserId) -> 
                async { 
                    return  Ok()
                }
        member _.Verify: Verify = 
            commandApi.Verify

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

    