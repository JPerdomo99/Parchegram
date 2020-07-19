using Parchegram.Service.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ParchegramTest
{
    public class User_Should
    {
        private readonly UserService _userService;

        public User_Should()
        {
            _userService = new UserService();
        }

        [Fact]
        public void NameUserUnique_ReturnFalse()
        {
            bool result = _userService.NameUserUnique("Julian1999");

            Assert.False(result, "Nombre de usuario ocupado");
        }

        [Fact]
        public void EmailUnique_ReturnFalse()
        {
            bool result = _userService.EmailUnique("atehortua199@gmail.com");

            Assert.False(result, "El email ya esta en uso");
        }
    }
}
