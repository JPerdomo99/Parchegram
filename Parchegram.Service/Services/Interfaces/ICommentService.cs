using Parchegram.Model.Request.Comment;
using Parchegram.Model.Response.General;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface ICommentService
    {
        public Task<Response> PostComment(PostCommentRequest postCommentRequest);

        public Task<Response> GetCommentsByPost(int idPost, int limit = 0);

        public Task<Response> UpdateComment(UpdateCommentRequest updateCommentRequest);

        public Task<Response> DeleteComment(DeleteCommentRequest deleteCommentRequest);
    }
}
