using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Email
{
    public class EmailRequest
    {
        public EmailRequest(string email, string codeConfirmEmail)
        {
            Email = email;
            CodeConfirmEmail = codeConfirmEmail;
        }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [MaxLength(128)]
        public string CodeConfirmEmail { get; set; }
    }
}
