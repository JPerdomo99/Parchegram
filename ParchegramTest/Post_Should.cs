using Parchegram.Model.Models;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.Post;
using Parchegram.Service.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ParchegramTest
{
    public class Post_Should
    {
        private readonly PostService _postService;

        public Post_Should()
        {
            _postService = new PostService();
        }

        //[Fact]
        //public void CreatePost_ReturnTrue()
        //{
        //    CreatePostRequest createPostRequest = new CreatePostRequest("Primer post bien suavata", "", 1, 3);
        //    bool result = _postService.CreatePost(createPostRequest);

        //    Assert.True(result, "Se guardo en la db"); 
        //}

        //[Fact]
        public void EditPost_ReturnTrue()
        {
            EditPostRequest editPostRequest = new EditPostRequest(1, "Primer post editado bien suavata", "", 3);
            bool result = _postService.EditPost(editPostRequest);

            Assert.True(result, "Se actualizo el post en la db");
        }

        //[Fact]
        public void DeletePost_ReturnTrue()
        {
            bool result = _postService.DeletePost(2);

            Assert.True(result, "Se elimino el post de la db");
        }

        //[Fact]
        public void GetPostById()
        {
            PostResponse post = _postService.GetPostById(1);

            Assert.NotNull(post);
        }
    }
}
