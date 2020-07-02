using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Parchegram.Model.Common;
using Parchegram.Model.Response;
using Parchegram.Model.User.Request;
using Parchegram.Service.Services.Interfaces;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;

        public UserController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        [HttpGet("GetFrase")]
        [Authorize]
        public string GetFrase()
        {
            return "Holaa";
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            Response response = new Response();
            if (ModelState.IsValid)
            {
                var userResponse = _userService.Login(model, _appSettings);
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
            } else
            {
                return BadRequest(ModelState);
            }

            return Ok(response);
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterRequest model)
        {
            Response response = new Response();
            if (ModelState.IsValid)
            {
                var userResponse = _userService.Register(model, _appSettings);
                if (userResponse == null)
                {
                    response.Message = "User already exists";
                    response.Success = 0;
                    response.Data = userResponse;
                    return BadRequest();
                }

                response.Message = "Register succcess!";
                response.Success = 1;
                response.Data = userResponse;
            } else
            {
                return BadRequest(ModelState);
            }

            return Ok(response);
        }
    }
}
