using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Post
{
    public class PostResponse : PostCommentsResponse
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public string PathFile { get; set; }

        public DateTime Date { get; set; }

        public int IdTypePost { get; set; }

        public int IdUser { get; set; }

        public string NameUser { get; set; }

        public int NumLikes { get; set; }
    }
}
