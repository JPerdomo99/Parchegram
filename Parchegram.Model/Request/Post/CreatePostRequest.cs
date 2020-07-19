using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;                  

namespace Parchegram.Model.Post.Request
{
    public class CreatePostRequest
    {
        public CreatePostRequest()
        {
        }

        public CreatePostRequest(string description, IFormFile file, int idUser, int idTypePost)
        {
            Description = description;
            File = file;
            IdUser = idUser;
            IdTypePost = idTypePost;
        }

        [MaxLength(3000)]
        public string Description { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdTypePost { get; set; }
    }
}
