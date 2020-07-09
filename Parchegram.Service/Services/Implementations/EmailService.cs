using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Email;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;

        public EmailService()
        {
        }

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public bool ConfirmEmail(string codeConfirmEmail)
        {
            using (var db = new ParchegramDBContext())
            {
                User user = db.User.Where(u => u.CodeConfirmEmail == codeConfirmEmail).FirstOrDefault();
                if (user != null)
                {
                    if (!user.ConfirmEmail)
                    {
                        user.ConfirmEmail = true;
                        db.User.Update(user);
                        if (db.SaveChanges() == 1)
                            return true;

                        return false;
                    }
                    return false;
                }
                return false;
            }
        }

        public void SendEmail(EmailRequest emailRequest)
        {
            try
            {
                // Construcción del html
                var builder = new BodyBuilder();
                builder.HtmlBody = $@"<h2 style='color: #409EFF'>Parchegram comparte tus mejores momentos</h2></br>
    <p>Estamos encantados de que compartas tu mejores momentos en Parchegram</p>
    <p>Pero antes de eso confirma tu email :)</p>
    <a style='color:#409EFF;text-decoration: none;font-size:16px' href='http://localhost:8080/confirmEmail/{emailRequest.CodeConfirmEmail}'>Confirmar Email!</a>";

                // Construcción del mensaje
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Equipo de parchegram julian", "atehortuaperdomo@gmail.com"));
                message.To.Add(new MailboxAddress(emailRequest.Email, emailRequest.Email));

                // Asunto
                message.Subject = "Verificación de email";
                // Cuerpo del correo
                message.Body = builder.ToMessageBody();

                // Envio del correo
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("atehortuaperdomo@gmail.com", "atehortua1151970207");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }
    }
}
