using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Comment;
using Parchegram.Model.Response.Comment;
using Parchegram.Model.Response.General;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ILogger _logger;

        public CommentService(ILogger<CommentService> logger)
        {
            _logger = logger;
        }

        public CommentService()
        {
        }

        public async Task<Response> GetCommentsByPost(int idPost, int limit = 0)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post post = await db.Post.Where(p => p.Id.Equals(idPost)).FirstOrDefaultAsync();
                    if (post == null)
                        return response.GetResponse("El post no existe", 0, null);

                    ICollection<PostCommentResponse> postCommentResponses;
                    if (limit > 0)
                    {
                        postCommentResponses = await
                                                (from comment in db.Comment
                                                join user in db.User on comment.IdUser equals user.Id
                                                where comment.IdPost == idPost
                                                orderby comment.Date
                                                select new PostCommentResponse
                                                {
                                                    IdComment = comment.Id,
                                                    NameUser = user.NameUser,
                                                    CommentText = comment.CommentText,
                                                    Date = comment.Date
                                                }).OrderByDescending(c => c.Date).Take(limit).ToListAsync();
                     
                        return response.GetResponse("Exito al obtener los comentarios con limite", 1, postCommentResponses);
                    }
                    postCommentResponses = await
                                            (from comment in db.Comment
                                            join user in db.User on comment.IdUser equals user.Id
                                            where comment.IdPost == idPost
                                            orderby comment.Date
                                            select new PostCommentResponse
                                            {
                                                IdComment = comment.Id,
                                                NameUser = user.NameUser,
                                                CommentText = comment.CommentText,
                                                Date = comment.Date
                                            }).OrderByDescending(c => c.Date).ToListAsync();

                    return response.GetResponse("Exito al obtener los comentarios sin limite", 1, postCommentResponses);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return response.GetResponse($"Ha ocurrido un error inesperado {e.Message}", 0, null);
                }
            }
        }

        public async Task<Response> PostComment(PostCommentRequest postCommentRequest)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post post = db.Post.Where(p => p.Id == postCommentRequest.IdPost).FirstOrDefault();
                    if (post == null)
                        return response.GetResponse("El post no existe", 0, false);

                    User user = db.User.Where(u => u.NameUser == postCommentRequest.NameUser).FirstOrDefault();
                    if (user == null)
                        return response.GetResponse("El usuario no existe", 0, false);

                    Comment comment = new Comment();
                    comment.IdUser = user.Id;
                    comment.IdPost = postCommentRequest.IdPost;
                    comment.CommentText = postCommentRequest.CommentText;
                    comment.Date = DateTime.Now;

                    db.Comment.Add(comment);
                    if (await db.SaveChangesAsync() == 1)
                        return response.GetResponse("Comentario guardado con exito", 1, true);
                    else
                        return response.GetResponse("El comentario no se guardo correctamente", 0, false);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return response.GetResponse($"Hubo un error inesperado {e.Message}", 0, false);
                }
            }
        }

        public bool DeleteComment(DeleteCommentRequest deleteCommentRequest)
        {
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    Comment oComment = db.Comment.Where(c => c.IdPost == deleteCommentRequest.IdPost 
                                        && c.IdUser == deleteCommentRequest.IdUser).FirstOrDefault();
                    db.Comment.Remove(oComment);

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
    }
}
