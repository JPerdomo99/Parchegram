using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Comment
{
    public class PostCommentRequest
    {
        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdPost { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(1200)]
        public string CommentText { get; set; }
    }
}
