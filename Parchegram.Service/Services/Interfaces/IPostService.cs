using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.Post;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IPostService
    {
        public Task<Response> CreatePost(CreatePostRequest createPostRequest);

        public bool EditPost(EditPostRequest editPostRequest);

        public bool DeletePost(int id);

        public Task<ICollection<PostResponse>> GetPostList(string nameUser);

        public PostResponse GetPostById(int id);
    }
}
