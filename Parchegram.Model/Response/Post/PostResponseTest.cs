using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Post
{
    class PostResponseTest
    {
        public int IdPost { get; set; }

        public byte[] File { get; set; }

        public int IdTypePost { get; set; }

        public DateTime Date { get; set; }

        // -----------------------------------
        public int IdUserFollowing { get; set; }

        public string NameUserFollowing { get; set; }

        public byte[] ImageProfileFollowing { get; set; }

        // -----------------------------------
        public int IdUserShare { get; set; }

        public string NameUserShare { get; set; }

        public byte[] ImageProfileShare { get; set; }
    }
}
