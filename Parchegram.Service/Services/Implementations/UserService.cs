using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Parchegram.Model.Common;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Email;
using Parchegram.Model.Request.User;
using Parchegram.Model.Response;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.User;
using Parchegram.Model.User.Request;
using Parchegram.Service.ClassesSupport;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Service.Tools;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<UserResponse> Login(LoginRequest loginRequest, AppSettings appSettings)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    var user = (from userLogin in db.User
                                join userImageProfile in db.UserImageProfile on userLogin.Id equals userImageProfile.IdUser into leftUserImageProfile
                                from subUserImageProfile in leftUserImageProfile.DefaultIfEmpty()
                                where userLogin.NameUser == loginRequest.NameUser && userLogin.Password == Encrypt.GetSHA256(loginRequest.Password)
                                select new { UserLoginResult = userLogin, UserImageProfileResult = subUserImageProfile }).FirstOrDefault();

                    if (user == null)
                        return null;

                    UserResponse userResponse = new UserResponse();
                    userResponse.NameUser = user.UserLoginResult.NameUser;
                    userResponse.Email = user.UserLoginResult.Email;
                    if (user.UserImageProfileResult != null)
                    {
                        Image image = new Image();
                        userResponse.ImageProfile = await image.GetFile(user.UserImageProfileResult.PathImageS);
                    }
                    userResponse.Token = GetToken(userResponse, appSettings);

                    return userResponse;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Metodo que es llamado para guardar un registro de una nuevo usuario
        /// </summary>
        /// <param name="registerRequest">Modelo que contiene los datos para registrar el nuevo usuario</param>
        /// <returns>Un objeto Response que define el exito de la operación</returns>
        public async Task<Response> Register(RegisterRequest registerRequest)
        {
            UserResponse userResponse = new UserResponse();
            using (var db = new ParchegramDBContext())
            {
                Response response = new Response();
                try
                {
                    User userEmail = await db.User.Where(u => u.Email.Equals(registerRequest.Email)).FirstOrDefaultAsync();
                    User userNameUser = await db.User.Where(u => u.NameUser.Equals(registerRequest.NameUser)).FirstOrDefaultAsync();
                    // Los dos son datos unicos, no puede haber otro con el mismo Email y no puede haber otro con el mismo Nombre
                    if (userEmail != null && userNameUser != null)
                    {
                        User user = new User();
                        user.NameUser = registerRequest.NameUser;
                        user.Email = registerRequest.Email;
                        user.Password = Encrypt.GetSHA256(registerRequest.Password);
                        user.DateBirth = registerRequest.DateBirth;
                        user.CodeConfirmEmail = Guid.NewGuid().ToString();
                        db.User.Add(user);
                        if (await db.SaveChangesAsync() == 1)
                            return response.GetResponse("Se ha guardado el usuario correctamente", 1, true);

                        return response.GetResponse("No se pudo guardar el usuario", 0, false);
                    } else
                    {
                        string datoUsuario = userEmail != null ? datoUsuario = "Email" : (userNameUser != null ? datoUsuario = "Name User" : "");
                        return response.GetResponse($"Ya hay un usuario registrado con ese dato: {datoUsuario}", 0, false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return response.GetResponse($"Ha ocurrido un error al intentar guardar el usuario: {e.Message}", 0, false);
                }
            }
        }

        /// <summary>
        /// Verifica que no halla un usuario registrado con ese mismo NameUser
        /// </summary>
        /// <param name="nameUser">NameUser del usuario que aspira registrarse</param>
        /// <returns></returns>
        public Response NameUserUnique(string nameUser)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == nameUser).FirstOrDefault();
                    if (user == null)
                    {
                        response.Success = 1;
                        response.Data = true;
                        response.Message = "Nombre disponible";

                        return response;
                    }

                    response.Success = 0;
                    response.Data = false;
                    response.Message = "Nombre ocupado";

                    return response;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Error inesperado {e.Message}";

                    return response;
                }
            }
        }

        /// <summary>
        /// Verifica que no halla un usuario registrado con ese mismo Email
        /// </summary>
        /// <param name="email">Email del usuario que aspira registrarse</param>
        /// <returns>Response</returns>
        public Response EmailUnique(string email)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.Email == email).FirstOrDefault();
                    if (user == null)
                    {
                        response.Success = 1;
                        response.Data = true;
                        response.Message = "Email Disponible";

                        return response;
                    }

                    response.Success = 0;
                    response.Data = false;
                    response.Message = "Email ocupado";

                    return response;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Error inesperado {e.Message}";

                    return response;
                }
            }
        }

        /// <summary>
        /// Método que verifica la existencia del usuario cuando intenta logearse
        /// </summary>
        /// <param name="loginRequest">Modelo que mapea los datos del login</param>
        /// <returns>Response</returns>
        public async Task<Response> UserExists(LoginRequest loginRequest)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = await db.User.Where(u => u.NameUser == loginRequest.NameUser && u.Password ==
                                Encrypt.GetSHA256(loginRequest.Password)).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        response.Success = 0;
                        response.Data = false;
                        response.Message = "El usuario no existe";

                        return response;
                    }

                    response.Success = 1;
                    response.Data = true;
                    response.Message = "El usuario existe";

                    return response;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Error inespareado {e.Message}";

                    return response;
                }
            }
        }

        /// <summary>
        /// Método que verifica que una cuenta ha sido verificada por email
        /// </summary>
        /// <param name="nameUser">NameUser para consultar el usuario</param>
        /// <returns>Response</returns>
        public async Task<Response> EmailConfirmed(string nameUser)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = await db.User.Where(u => u.NameUser == nameUser).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        if (user.ConfirmEmail == true)
                        {
                            response.Success = 1;
                            response.Data = true;
                            response.Message = "Email confirmado";

                            return response;
                        }

                        response.Success = 0;
                        response.Data = false;
                        response.Message = "Email no confirmado";

                        return response;
                    }

                    response.Success = 0;
                    response.Data = false;
                    response.Message = "Usuario no existe";

                    return response;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Error inespareado {e.Message}";

                    return response;
                }
            }
        }

        /// <summary>
        /// Entrada del mentodo para llevar a cabo los procesos correspondientes para 
        /// configurar la cuenta de usuario
        /// </summary>
        /// <param name="configUserRequest">Modelo requerido</param>
        /// <returns>Respuesta que confirma el exito del proceso</returns>
        public async Task<Response> UserConfig(ConfigUserRequest configUserRequest)
        {
            Response response = new Response();

            // Validamos la imagen (extensión y tamaño)
            if (configUserRequest.ImageProfile != null)
            {
                if (!ValidateFile.ValidateExtensionImage(configUserRequest.ImageProfile.ContentType))
                {
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Formato de imagen no valido {configUserRequest.ImageProfile.ContentType}";

                    return response;
                }
                if (!ValidateFile.ValidateSizeFile(configUserRequest.ImageProfile.Length, 5000000))
                {
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Máximo 5MB para el archivo: {ValidateFile.ConvertToMegabytes(configUserRequest.ImageProfile.Length)}";

                    return response;
                }
            }

            using (var db = new ParchegramDBContext())
            {
                try
                {
                    var user = await db.User.Where(u => u.NameUser == configUserRequest.NameUserToken).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        if (configUserRequest.NameUser != null)
                        {
                            user.NameUser = configUserRequest.NameUser;
                            await db.SaveChangesAsync();
                        }
                        if (configUserRequest.ImageProfile != null)
                        {
                            ImageUserProfile imageUserProfile = new ImageUserProfile(true);
                            imageUserProfile.SaveProfileImage(configUserRequest.ImageProfile, user.NameUser);
                        }
                    }
                    else
                    {
                        response.Success = 0;
                        response.Data = false;
                        response.Message = "El usuario no existe";

                        return response;
                    }

                    response.Success = 1;
                    response.Data = true;
                    response.Message = "Usuario actualizado correctamente";

                    return response;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = false;
                    response.Message = $"Ha ocurrido un error {e.Message}";

                    return response;
                }
            }
        }

        /// <summary>
        /// Metodo que devuelve un response con datos del Usuario recien actualizado
        /// para almacenar en la sessión del cliente
        /// </summary>
        /// <param name="nameUser">NameUser del usuario que se ha actualizado</param>
        /// <returns>Respuesta que contiene datos del usuario</returns>
        public async Task<Response> GetUserConfigResponse(ConfigUserRequest configUserRequest)
        {
            Response response = new Response();

            using (var db = new ParchegramDBContext())
            {
                try
                {
                    var userConfig = (from user in db.User
                                      join userImageProfile in db.UserImageProfile on user.Id equals userImageProfile.IdUser into leftUserImageProfile
                                      from subUserImageProfile in leftUserImageProfile.DefaultIfEmpty()
                                      where user.NameUser == configUserRequest.NameUser || user.NameUser == configUserRequest.NameUserToken
                                      select new { user.NameUser, subUserImageProfile.PathImageS }).FirstOrDefault();

                    if (userConfig == null)
                    {
                        response.Success = 0;
                        response.Data = null;
                        response.Message = "El usuario no existe";

                        return response;
                    }

                    UserConfigResponse userConfigResponse = new UserConfigResponse();
                    userConfigResponse.NameUser = userConfig.NameUser;
                    try
                    {
                        Image image = new Image();
                        userConfigResponse.ImageProfile = await image.GetFile(userConfig.PathImageS);
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(e.Message);
                        userConfigResponse.ImageProfile = null;
                    }

                    response.Success = 1;
                    response.Data = userConfigResponse;
                    response.Message = "Datos obtenidos correctamente";
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    response.Success = 0;
                    response.Data = null;
                    response.Message = $"Error al obtener los datos {e.Message}";
                    throw;
                }
            }

            return response;
        }

        /// <summary>
        /// Crea el token segun el Usuario
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="appSettings">Llave secreta para firmar el token</param>
        /// <returns>Token</returns>
        private string GetToken(UserResponse userResponse, AppSettings appSettings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {   // Datos que se van a incluir en el token
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Email, userResponse.Email),
                        new Claim(ClaimTypes.UserData, userResponse.NameUser)
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
