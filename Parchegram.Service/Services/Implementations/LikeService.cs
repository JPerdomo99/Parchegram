using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Like;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly ILogger _logger;

        public LikeService(ILogger<LikeService> logger)
        {
            _logger = logger;
        }

        public LikeService()
        {

        }

        public bool AddLike(LikeRequest likeRequest)
        {
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    Like oLike = new Like();
                    oLike.IdPost = likeRequest.IdPost;
                    oLike.IdUser = likeRequest.IdUser;
                    db.Like.Add(oLike);

                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return false;
            }
        }

        public bool DeleteLike(LikeRequest likeRequest)
        {
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    Like oLike = db.Like.Where(l => l.IdPost == likeRequest.IdPost && l.IdUser == likeRequest.IdUser).FirstOrDefault();
                    db.Remove(oLike);

                    
                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return false;
            }
        }

        public int GetNumLikes(int idPost)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    return db.Like.Where(l => l.IdPost == idPost).Count();
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return 0;
                }
            }
        }
    }
}
