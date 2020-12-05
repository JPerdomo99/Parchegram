using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("Get/{sender}/{receiver}")]
        public async Task<IActionResult> Get([FromRoute] string sender, [FromRoute] string receiver)
        {
            Response response = await _messageService.GetMessages(sender, receiver);
            return Ok(response);
        }
    }
}
