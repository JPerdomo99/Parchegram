using System.Collections.Immutable;

namespace Parchegram.Model.Response.Post
{
    public class PostListPaginateResponse
    {
        public IImmutableList<PostResponse> PostList { get; set; }

        public int TotalRows { get; set; }

        public PostListPaginateResponse(IImmutableList<PostResponse> postList, int totalRows)
        {
            PostList = postList;
            TotalRows = totalRows;
        }
    }
}
