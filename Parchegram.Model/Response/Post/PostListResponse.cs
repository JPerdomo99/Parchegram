using Parchegram.Model.Models;
using Parchegram.Model.Response.Post;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response
{
    public class PostListResponse : PostCommentsResponse
    {
        // Post
        public int IdPost { get; set; }

        public int IdTypePost { get; set; }

        public string Description { get; set; }

        public string PathFile { get; set; }

        public byte[] File { get; set; }

        public DateTime Date { get; set; }

        // UserOwnerPost
        public int IdUserOwnerPost { get; set; }

        public string NameUserOwnerPost { get; set; }

        // UserSharePost
        public int IdUserSharePost { get; set; }

        public string NameUserUserSharePost { get; set; }

        // NumLikes
        public int NumLikes { get; set; }
    }
}
