using Parchegram.Model.Request.Email;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(EmailRequest emailRequest);

        public bool ConfirmEmail(string codeConfirmEmail);
    }
}
