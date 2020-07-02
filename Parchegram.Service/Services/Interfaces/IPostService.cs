using Parchegram.Model.Models;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Comment;
using Parchegram.Model.Request.Like;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.Post;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IPostService
    {
        public bool CreatePost(CreatePostRequest createPostRequest);
        public bool EditPost(EditPostRequest editPostRequest);
        public bool DeletePost(int id);
        public ICollection<PostResponse> GetPostList(int idUser);
        public PostResponse GetPostById(int id);
    }
}
