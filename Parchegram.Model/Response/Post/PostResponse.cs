using System;

namespace Parchegram.Model.Response.Post
{
    public class PostResponse
    {
        // Post
        public int IdPost { get; set; }

        public int IdTypePost { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public byte[] File { get; set; }

        // UserOwner
        public int IdUserOwner { get; set; }

        public string NameUserOwner { get; set; }

        public byte[] ImageProfileUserOwner { get; set; }

        // UserShare
        public int IdUserShare { get; set; }

        public string NameUserShare { get; set; }

        public byte[] ImageProfileUserShare { get; set; }

        // If the user liked the publication
        public bool LikeUser { get; set; }

        // NumLikes
        public int NumberLikes { get; set; }
    }
}
