using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Cryptography;
using Parchegram.Model.Common;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Email;
using Parchegram.Model.Request.User;
using Parchegram.Model.Response;
using Parchegram.Model.User.Request;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Service.Tools;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;

        public UserService()
        {

        }

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public UserResponse Login(LoginRequest loginRequest, AppSettings appSettings)
        {
            UserResponse userReponse = new UserResponse();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == loginRequest.NameUser
                            && u.Password == Encrypt.GetSHA256(loginRequest.Password)).FirstOrDefault();
                    if (user == null)
                        return null;

                    userReponse.NameUser = user.NameUser;
                    userReponse.Email = user.Email;
                    userReponse.Token = GetToken(user, appSettings);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }

            return userReponse;
        }

        public UserResponse Register(RegisterRequest registerRequest, AppSettings appSettings)
        {
            UserResponse userResponse = new UserResponse();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.Email == registerRequest.Email || u.NameUser == registerRequest.NameUser).FirstOrDefault();
                    if (user == null)
                    {
                        User oUser = new User();
                        oUser.NameUser = registerRequest.NameUser;
                        oUser.Email = registerRequest.Email;
                        oUser.Password = Encrypt.GetSHA256(registerRequest.Password);
                        oUser.DateBirth = registerRequest.DateBirth;
                        oUser.CodeConfirmEmail = Guid.NewGuid().ToString();
                        db.User.Add(oUser);
                        if (db.SaveChanges() == 1)
                        {
                            user = db.User.Where(u => u.Email == registerRequest.Email).FirstOrDefault();

                            userResponse.Email = registerRequest.Email;
                            userResponse.NameUser = registerRequest.NameUser;
                            userResponse.Token = GetToken(user, appSettings);

                            EmailRequest emailRequest = new EmailRequest(user.Email, user.CodeConfirmEmail);
                            IEmailService emailService = new EmailService();
                            emailService.SendEmail(emailRequest);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }

            return userResponse;
        }

        // Se verifica que el nombre de usuario no este ocupado
        public bool NameUserUnique(string nameUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == nameUser).FirstOrDefault();
                    if (user == null)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        // Se verifica que el email no este registrado
        public bool EmailUnique(string email)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.Email == email).FirstOrDefault();
                    if (user == null)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        // Metodo para confirmar la existencia del usuario 
        // intenta logearse
        public bool UserExists(LoginRequest loginRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == loginRequest.NameUser && u.Password == Encrypt.GetSHA256(loginRequest.Password)).FirstOrDefault();
                    if (user == null)
                        return false;

                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        // Metodo que nos permite confirmar la confirmación 
        // de un email
        public bool EmailConfirmed(string nameUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == nameUser).FirstOrDefault();

                    if (user != null)
                        if (user.ConfirmEmail == true)
                            return true;
                        else
                            return false;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
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

        #region TestImageProfile
        /// <summary>
        /// Entrada del mentodo para llevar a cabo los procesos correspondientes
        /// </summary>
        /// <param name="configUserRequest"></param>
        /// <returns>Bool</returns>
        public bool UserConfig(ConfigUserRequest configUserRequest)
        {
            byte[] imageProfile = ConvertToByteArray(configUserRequest.ImageProfile);

            return true;
        }

        private byte[] ConvertToByteArray(IFormFile formFile)
        {
            byte[] fileBytes = null;
            if (formFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    fileBytes = ms.ToArray();
                    string s = Convert.ToBase64String(fileBytes);
                    // act on the Base64 data
                }
            }

            return fileBytes;
        }
        #endregion
    }
}
