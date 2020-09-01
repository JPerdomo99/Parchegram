using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Request.Like;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System.Threading.Tasks;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] LikeRequest likeRequest)
        {
            if (ModelState.IsValid)
            {
                Response result = await _likeService.AddLike(likeRequest);
                return Ok(result);
            } else
            {
                Response response = new Response();
                return BadRequest(response.GetResponse("Modelo no válido", 0, false));
            }
        }

        [HttpDelete("Delete/{idPost}/{nameUser}")]
        public async Task<IActionResult> Delete([FromRoute] int idPost, [FromRoute] string nameUser)
        {
            Response result = await _likeService.DeleteLike(idPost, nameUser);
            return Ok(result);
        }
    }
}