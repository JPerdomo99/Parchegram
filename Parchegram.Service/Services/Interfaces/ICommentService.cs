using Parchegram.Model.Request.Comment;
using Parchegram.Model.Response.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    public interface ICommentService
    {
        public bool PostComment(PostCommentRequest postCommentRequest);

        public ICollection<PostCommentResponse> GetCommentsByPost(int idPost, bool byId);

        public bool DeleteComment(DeleteCommentRequest deleteCommentRequest);
    }
}
