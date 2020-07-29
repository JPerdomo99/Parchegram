using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Post
{
    public class EditPostRequest
    {
        public EditPostRequest()
        {
        }

        public EditPostRequest(int id, string description, string pathFile, int idTypePost)
        {
            Id = id;
            Description = description;
            PathFile = pathFile;
            IdTypePost = idTypePost;
        }

        [Required]
        public int Id { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Upload)]
        public string PathFile { get; set; }

        [Required]
        public int IdTypePost { get; set; }
    }
}
