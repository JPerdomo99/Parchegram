using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.Post;
using Parchegram.Service.Services.Implementations;
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

        ////[Fact]
        //public void EditPost_ReturnTrue()
        //{
        //    EditPostRequest editPostRequest = new EditPostRequest(1, "Primer post editado bien suavata", "", 3);
        //    bool result = _postService.EditPost(editPostRequest);

        //    Assert.True(result, "Se actualizo el post en la db");
        //}

        ////[Fact]
        //public void DeletePost_ReturnTrue()
        //{
        //    bool result = _postService.DeletePost(2);

        //    Assert.True(result, "Se elimino el post de la db");
        //}

        ////[Fact]
        //public void GetPostById()
        //{
        //    PostResponse post = _postService.GetPostById(1);

        //    Assert.NotNull(post);
        //}

        //[Fact]
        //public async void GetPostList_NoNull()
        //{
        //    Response response = await _postService.GetPostList("Julian9999");
        //    Assert.NotNull(response.Data);
        //}

        //[Fact]
        //public async void GetPostList_Null()
        //{
        //    Response response = await _postService.GetPostList("NoExists");
        //    Assert.Null(response);
        //}
    }
}
