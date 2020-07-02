using Parchegram.Model.Request.Like;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    public interface ILikeService
    {
        public bool AddLike(LikeRequest likeRequest);
        public int GetNumLikes(int idPost);
        public bool DeleteLike(LikeRequest likeRequest);
    }
}
