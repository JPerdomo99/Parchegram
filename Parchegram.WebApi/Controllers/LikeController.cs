using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Request.Like;
using System.Threading.Tasks;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        [HttpGet("Add")]
        public async Task<IActionResult> Add(LikeRequest likeRequest)
        {

        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(LikeRequest likeRequest)
        {

        }
    }
}