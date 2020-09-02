using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Post
{
    public class PostListPaginateResponse
    {
        public ICollection<PostResponse> PostList { get; set; }

        public int CurrentPage { get; set; }

        public PostListPaginateResponse(ICollection<PostResponse> postList, int currentPage)
        {
            PostList = postList;
            CurrentPage = currentPage;
        }
    }
}
    