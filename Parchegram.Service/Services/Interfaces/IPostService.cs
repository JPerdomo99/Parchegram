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

        public Task<Response> GetPostList(string nameUser, int page);

        public Task<Response> GetPostList(string nameUser, int page, int idTypePost, string nameUserSession);

        public Task<Response> GetPostById(int id);
    }
}
