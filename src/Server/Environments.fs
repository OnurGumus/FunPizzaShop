module FunPizzaShop.Server.Environments
open FunPizzaShop
open FunPizzaShop.ServerInterfaces
open Microsoft.Extensions.Configuration
open Query
open Command
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza
open System.Net.Mail
open System.Net
open Serilog

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) as self =
    let commandApi =
        lazy(Command.API.api self NodaTime.SystemClock.Instance)

    let queryApi =
        lazy(Query.API.api config commandApi.Value.ActorApi)
        
    interface IMailSender with
        member _.SendVerificationMail =
            fun (email:Email) (subject: Subject) (body: Body) ->
                Log.Debug("Sending mail to {email}: {@body}", email, body)
                async{
                    let sender = "info@bindrake.com"
                    let password = config.GetSection("config:SendGrid:APIKEY").Value
                    if password = null then
                        Log.Error("No SendGrid APIKEY found in config")
                        return ()
                    let target = email.Value
                    let subject = subject.Value
                    let body = body.Value
                    let userName = "apikey"
                    let msg = new MailMessage()
                    msg.To.Add(new MailAddress(target))
                    msg.From <- new MailAddress(sender)
                    msg.Subject <- subject
                    msg.Body <- body
                    msg.IsBodyHtml <- true

                    let client =
                        new SmtpClient(
                            Host = "smtp.sendgrid.net",
                            Port = 587,
                            EnableSsl = true,
                            Credentials = new NetworkCredential(userName, password)
                        )
                    client.Send(msg)
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

    