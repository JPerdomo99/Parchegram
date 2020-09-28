using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Comment
{
    public class PostCommentRequest
    {
        [Required]
        public string NameUser { get; set; }

        [Required]
        public int IdPost { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string CommentText { get; set; }
    }
}
