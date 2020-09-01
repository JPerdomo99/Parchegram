using Parchegram.Model.Request.Like;
using Parchegram.Model.Response.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface ILikeService
    {
        public Task<Response> AddLike(LikeRequest likeRequest);

        public Task<Response> DeleteLike(int idPost, string nameUser);

        public int GetNumLikes(int idPost);
    }
}
