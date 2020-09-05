using System.Collections.Immutable;

namespace Parchegram.Model.Response.Post
{
    public class PostListPaginateResponse
    {
        public IImmutableList<PostResponse> PostList { get; set; }

        public int CurrentPage { get; set; }

        public PostListPaginateResponse(IImmutableList<PostResponse> postList, int currentPage)
        {
            PostList = postList;
            CurrentPage = currentPage;
        }
    }
}
