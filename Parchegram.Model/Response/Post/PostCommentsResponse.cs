using Parchegram.Model.Response.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Post
{
    public class PostCommentsResponse
    {
        public ICollection<PostCommentResponse> CommentsByPost { get; set; }
    }
}
