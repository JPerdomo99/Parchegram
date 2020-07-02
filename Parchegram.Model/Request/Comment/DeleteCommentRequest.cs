using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Comment
{
    public class DeleteCommentRequest
    {
        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdPost { get; set; }
    }
}
