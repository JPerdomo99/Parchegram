using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Post
{
    public class EditPostRequest
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(3000)]
        public string Description { get; set; }

        [DataType(DataType.Upload)]
        public string PathFile { get; set; }

        [Required]
        public int IdTypePost { get; set; }
    }
}
