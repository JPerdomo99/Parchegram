using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parchegram.Model.Post.Request;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Model.Response.Post;
using Parchegram.Model.Response;
using System.Text.Json.Serialization;
using System.Text.Json;

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
        public IActionResult Create([FromForm] CreatePostRequest createPostRequest)
        {
            bool result = _postService.CreatePost(createPostRequest);

            return Ok(result);  
        }

        [HttpGet("GetPosts/{nameUser}")]
        public IActionResult GetPostList([FromRoute] string nameUser)
        {
            ICollection<PostListResponse> result = _postService.GetPostList(nameUser);

            return Ok(result);
        }
    }
}
