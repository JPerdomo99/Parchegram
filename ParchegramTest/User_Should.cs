using Parchegram.Model.Request.User;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Implementations;
using System;
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
            Response response = _userService.NameUserUnique("Julian1999");
            bool result = Convert.ToBoolean(response.Data);

            Assert.False(result, "Nombre de usuario ocupado");
        }

        [Fact]
        public void EmailUnique_ReturnFalse()
        {
            Response response = _userService.EmailUnique("atehortua199@gmail.com");
            bool result = Convert.ToBoolean(response.Data);

            Assert.False(result, "El email ya esta en uso");
        }

        [Fact]
        public async void UserConfig_ReturnFalse()
        {
            ConfigUserRequest configUserRequest = new ConfigUserRequest();
            configUserRequest.NameUserToken = "Julian1234";
            Response response = await _userService.UserConfig(configUserRequest);
            bool result = Convert.ToBoolean(response.Data);

            Assert.False(result, "El usuario no existe");
        }
    }
}
