using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.User
{
    public class ConfigUserRequest
    {
        [DataType(DataType.Upload)]
        public IFormFile ImageProfile { get; set; }

        [MinLength(1)]
        [MaxLength(40)]
        public string NameUser { get; set; }
    }
}
