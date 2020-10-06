using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System.Threading.Tasks;

namespace Parchegram.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IWebHostEnvironment _env;

        public PostController(IPostService postService,
            IWebHostEnvironment env)
        {
            _postService = postService;
            _env = env;
        }

        [HttpGet("GetPosts/{nameUser}/{page?}")]
        public async Task<IActionResult> GetPostList([FromRoute] string nameUser, [FromRoute] int page = 1)
        {
            Response result = await _postService.GetPostList(nameUser, page);
            return Ok(result);
        }

        [HttpGet("GetPostById/{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] int id)
        {
            Response result = await _postService.GetPostById(id);
            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest createPostRequest)
        {
            if (ModelState.IsValid)
            {
                Response result = await _postService.CreatePost(createPostRequest);
                return Ok(result);
            }
            Response response = new Response();
            return BadRequest(response.GetResponse("Modelo no válido", 0, false));
        }

        [HttpPost("ActionUpload")]
        public IActionResult ActionUpload()
        {
            return Ok(true);
        }
    }
}
