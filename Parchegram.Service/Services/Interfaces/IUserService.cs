using Parchegram.Model.Common;
using Parchegram.Model.User.Request;
using Parchegram.Model.Response;
using System;
using System.Collections.Generic;
using System.Text;
using Parchegram.Model.Request.User;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IUserService
    {
        public UserResponse Login(LoginRequest model, AppSettings appSettings);
        public UserResponse Register(RegisterRequest model, AppSettings appSettings);
        public bool NameUserUnique(string nameUser);
        public bool EmailUnique(string email);
        public bool UserExists(LoginRequest loginRequest);
        public bool EmailConfirmed(string nameUser);
        public bool UserConfig(ConfigUserRequest configUserRequest);
    }
}
