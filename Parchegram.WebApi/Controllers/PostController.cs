using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Response;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System.Collections.Generic;
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

        [HttpGet("GetFrase")]
        public string GetFrase()
        {
            return "Soy un hombre con muchos defectos, quizas por tratar de ser perfecto";
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest createPostRequest)
        {
            Response result = await _postService.CreatePost(createPostRequest);

            return Ok(result);
        }

        [HttpGet("GetPosts/{nameUser}")]
        public IActionResult GetPostList([FromRoute] string nameUser)
        {
            ICollection<PostListResponse> result = _postService.GetPostList(nameUser);

            return Ok(result);
        }

        [HttpPost("ActionUpload")]
        public IActionResult ActionUpload()
        {
            return Ok(true);
        }
    }
}
