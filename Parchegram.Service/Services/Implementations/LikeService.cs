using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Like;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Response> AddLike(LikeRequest likeRequest)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(likeRequest.NameUser)).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        Like like = await db.Like.Where(l => l.IdPost.Equals(likeRequest.IdPost) && l.IdUser.Equals(user.Id)).FirstOrDefaultAsync();
                        if (like == null)
                        {
                            like = new Like();
                            like.IdPost = likeRequest.IdPost;
                            like.IdUser = user.Id;
                            db.Like.Add(like);
                            if (db.SaveChangesAsync().Result.Equals(1))
                                return response.GetResponse("El like se ha agregado correctamente", 1, true);
                            return response.GetResponse("El like no se puedo agregar corractamente", 0, false);
                        }
                        return response.GetResponse("El like ya exite", 0, false);
                    }
                    return response.GetResponse("El usuario no existe", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado: {e.Message}", 0, false);
            }
        }

        public async Task<Response> DeleteLike(LikeRequest likeRequest)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(likeRequest.NameUser)).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        Like like = await db.Like.Where(l => l.IdPost.Equals(likeRequest.IdPost) && l.IdUser.Equals(user.Id)).FirstOrDefaultAsync();
                        if (like != null)
                        {
                            db.Remove(like);
                            if (db.SaveChangesAsync().Result.Equals(1))
                                return response.GetResponse("El like se ha eliminado correctamente", 1, true);
                            return response.GetResponse("El like no se ha podido eliminar correctamente", 0, false);
                        }
                        return response.GetResponse("El like no existe", 0, false); 
                    }
                    return response.GetResponse("El usuario no existe", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado: {e.Message}", 0, false);
            }
        }

        public int GetNumLikes(int idPost)
        {
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    return db.Like.Where(l => l.IdPost == idPost).Count();
                }   
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return 0;
            }
        }
    }
}
