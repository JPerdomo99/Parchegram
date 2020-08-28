using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Parchegram.Model.Common;
using Parchegram.Model.Request.User;
using Parchegram.Model.Response.General;
using Parchegram.Model.User.Request;
using Parchegram.Service.Services.Interfaces;
using System.Threading.Tasks;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;

        public UserController(IUserService userService, IEmailService emailService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _emailService = emailService;
            _appSettings = appSettings.Value;
        }

        [HttpGet("GetFrase")]
        [Authorize]
        public string GetFrase()
        {
            return "Holaa";
        }

        [HttpPost("Login")]
        public async  Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            Response response = new Response();
            if (ModelState.IsValid)
            {
                var userResponse = await _userService.Login(loginRequest, _appSettings);
                if (userResponse == null)
                {
                    response.Message = "User does not exist";
                    response.Success = 0;
                    response.Data = userResponse;
                    return BadRequest(response);
                }

                response.Message = "Login success!";
                response.Success = 1;
                response.Data = userResponse;
            }
            else
            {
                return BadRequest(ModelState);
            }

            return Ok(response);
        }

        [HttpPost("Register")]
        public async  Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (ModelState.IsValid)
            {
                Response response = await _userService.Register(model);
                return Ok(response);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("ConfirmEmail")]
        public IActionResult ConfirmEmail([FromBody] string codeConfirmEmail)
        {
            if (_emailService.ConfirmEmail(codeConfirmEmail))
                return Ok();

            return BadRequest();
        }

        [HttpPost("UserExists")]
        public async Task<IActionResult> UserExists([FromBody] LoginRequest loginRequest)
        {
            Response result = await _userService.UserExists(loginRequest);

            return Ok(result);
        }

        [HttpGet("EmailConfirmed/{nameUser}")]
        public async Task<IActionResult> EmailConfirmed([FromRoute] string nameUser)
        {
            Response result = await _userService.EmailConfirmed(nameUser);

            return Ok(result);
        }

        [HttpGet("NameUserUnique/{nameUser}")]
        public IActionResult NameUserUnique([FromRoute] string nameUser)
        {
            Response result = _userService.NameUserUnique(nameUser);

            return Ok(result);
        }

        [HttpGet("EmailUnique/{email}")]
        public IActionResult EmailUnique([FromRoute] string email)
        {
            Response result = _userService.EmailUnique(email);

            return Ok(result);
        }

        [HttpPost("UserConfig")]
        public async Task<IActionResult> UserConfig([FromForm] ConfigUserRequest configUserRequest)
        {
            Response response = await _userService.UserConfig(configUserRequest);

            if (response.Success == 0)
                return BadRequest(response);

            response = await _userService.GetUserConfigResponse(configUserRequest);

            return Ok(response);
        }

        [HttpPost("ActionUpload")]
        public IActionResult ActionUpload()
        {
            return Ok(true);
        }
    }
}
