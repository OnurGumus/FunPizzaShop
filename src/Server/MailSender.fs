module FunPizzaShop.Server.MailSender
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

let sendMail (config:IConfiguration) (email:Email) (subject: Subject) (body: Body) =
    Log.Debug("Sending mail to {email}: {@body}", email, body)
    async{
        let sender = "info@bindrake.com"
        let password = config.GetSection("config:SendGrid:APIKEY").Value
        if password = null then
            Log.Error("No SendGrid APIKEY found in config")
            return ()
        else
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