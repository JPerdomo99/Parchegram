using System;
using System.Collections.Generic;
using System.Text;
using Parchegram.Model.Models;

namespace Parchegram.Model.Response.Post
{
    public class PostListQueryResponse
    {
        public Models.Post QueryPost { get; set; }

        public Models.User QueryUserOwner { get; set; }

        public Share QueryShare { get; set; }

        public Follow QueryFollow { get; set; }

        public Models.User QueryUserShare { get; set; }
    }
}
