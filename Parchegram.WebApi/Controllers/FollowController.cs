using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService,
            IWebHostEnvironment env)
        {
            _followService = followService;
            _env = env;
        }

        [HttpGet("Add/{nameUserFollower}/{idUserFollowing}")]
        public async Task<IActionResult> Add([FromRoute] string nameUserFollower, [FromRoute] int idUserFollowing)
        {
            Response result = await _followService.Add(nameUserFollower, idUserFollowing);
            return Ok(result);
        }

        [HttpGet("Delete/{nameUserFollower}/{idUserFollowing}")]
        public async Task<IActionResult> Delete([FromRoute] string nameUserFollower, [FromRoute] int idUserFollowing)
        {
            Response result = await _followService.Delete(nameUserFollower, idUserFollowing);
            return Ok(result);
        }

        [HttpGet("Get/{nameUserFollower}")]
        public async Task<IActionResult> Get([FromRoute] string nameUserFollower)
        {
            Response result = await _followService.GetFollowing(nameUserFollower);
            return Ok(result);
        }
    }
}