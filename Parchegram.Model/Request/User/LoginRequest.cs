using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.User.Request
{
    public class LoginRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string NameUser { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; }
    }
}
