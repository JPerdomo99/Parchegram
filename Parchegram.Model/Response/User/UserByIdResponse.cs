using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.User
{
    public class UserByIdResponse
    {
        public int IdUser { get; set; }

        public string NameUser { get; set; }

        public string Email { get; set; }

        public byte[] ImageProfile { get; set; }

        public DateTime DateBirth { get; set; }

        public bool Follow { get; set; }
    }
}
