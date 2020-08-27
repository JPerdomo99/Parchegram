using Parchegram.Model.Common;
using Parchegram.Model.User.Request;
using Parchegram.Model.Response;
using System;
using System.Collections.Generic;
using System.Text;
using Parchegram.Model.Request.User;
using Parchegram.Model.Response.General;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserResponse> Login(LoginRequest model, AppSettings appSettings);

        public UserResponse Register(RegisterRequest model);

        public Response NameUserUnique(string nameUser);

        public Response EmailUnique(string email);

        public Task<Response> UserExists(LoginRequest loginRequest);

        public Task<Response> EmailConfirmed(string nameUser);

        public Task<Response> UserConfig(ConfigUserRequest configUserRequest);

        public Task<Response> GetUserConfigResponse(ConfigUserRequest configUserRequest);
    }
}
