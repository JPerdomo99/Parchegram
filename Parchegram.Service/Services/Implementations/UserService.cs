using Microsoft.IdentityModel.Tokens;
using Parchegram.Model.Common;
using Parchegram.Model.Models;
using Parchegram.Model.Response;
using Parchegram.Model.User.Request;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Service.Tools;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class UserService : IUserService
    {
        public UserResponse Login(LoginRequest model, AppSettings appSettings)
        {
            UserResponse userReponse = new UserResponse();
            using (var db = new ParchegramDBContext())
            {
                User user = db.User.Where(u => u.Email == model.Email).FirstOrDefault();
                if (user == null)
                    return null;
                
                userReponse.NameUser = user.NameUser;
                userReponse.Email = user.Email;
                userReponse.Token = GetToken(user, appSettings);
            }

            return userReponse;
        }

        public UserResponse Register(RegisterRequest model, AppSettings appSettings)
        {
            UserResponse userResponse = new UserResponse();
            using (var db = new ParchegramDBContext())
            {
                User user = db.User.Where(u => u.Email == model.Email).FirstOrDefault();
                if (user == null)
                {
                    User oUser = new User();
                    oUser.NameUser = model.NameUser;
                    oUser.Email = model.Email;
                    oUser.Password = Encrypt.GetSHA256(model.Password);
                    oUser.DateBirth = model.DateBirth;
                    db.User.Add(oUser);
                    db.SaveChanges();

                    user = db.User.Where(u => u.Email == model.Email).FirstOrDefault();

                    userResponse.Email = model.Email;
                    userResponse.NameUser = model.NameUser;
                    userResponse.Token = GetToken(user, appSettings);
                }
                else
                {
                    return null;
                }
            }

            return userResponse;
        }

        private string GetToken(User user, AppSettings appSettings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {   // Datos que se van a incluir en el token
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.UserData, user.NameUser)
                    }
                ),
                // Expiración del token
                Expires = DateTime.UtcNow.AddDays(60),
                // Hacemos que se encripte la información                                    // Firmamos el token con este algoritmo
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
