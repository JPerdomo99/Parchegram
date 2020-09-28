using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Parchegram.Model.Request.Post
{
    public class CreatePostRequest
    {
        public CreatePostRequest()
        {
        }

        public CreatePostRequest(string description, IFormFile file, string nameUser, int idTypePost)
        {
            Description = description;
            File = file;
            NameUser = nameUser;
            IdTypePost = idTypePost;
        }

        [MaxLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }

        public string NameUser { get; set; }

        [Required]
        public int IdTypePost { get; set; }
    }
}
