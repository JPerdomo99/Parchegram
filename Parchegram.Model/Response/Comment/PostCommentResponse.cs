using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Comment
{
    public class PostCommentResponse
    {
        public int IdComment { get; set; }

        public string NameUser { get; set; }

        public string CommentText { get; set; }

        public DateTime Date { get; set; }
    }
}
