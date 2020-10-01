using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Comment
{
    public class DeleteCommentRequest
    {
        public DeleteCommentRequest(int idPost, int idComment, string nameUser)
        {
            IdPost = idPost;
            IdComment = idComment;
            NameUser = nameUser;
        }

        [Required]
        public int IdPost { get; set; }

        [Required]
        public int IdComment { get; set; }

        [Required]
        public string NameUser { get; set; }
    }
}
