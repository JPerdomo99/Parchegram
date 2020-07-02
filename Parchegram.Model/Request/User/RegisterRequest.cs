using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.User.Request
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(1)]
        [MaxLength(40)]
        public string NameUser { get; set; }

        [Required]
        [EmailAddress]
        [MinLength(5)]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateBirth { get; set; }
    }
}
