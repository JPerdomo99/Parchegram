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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idPost"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creamos un nuevo comentario
        /// </summary>
        /// <param name="postCommentRequest">Modelo mepeado con los datos para crear el nuevo comentario</param>
        /// <returns>Response</returns>
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

        /// <summary>
        /// Editar el comentario
        /// </summary>
        /// <param name="updateCommentRequest">Modelo mepeado con los datos para crear el nuevo comentario</param>
        /// <returns>Response</returns>
        public async Task<Response> UpdateComment(UpdateCommentRequest updateCommentRequest)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    response = await ValidatorSecurityComment(updateCommentRequest.NameUser, updateCommentRequest.IdPost, updateCommentRequest.IdComment);
                    if (response.Success.Equals(0))
                        return response;

                    Comment comment = await db.Comment.Where(c => c.Id.Equals(updateCommentRequest.IdComment)).FirstOrDefaultAsync();
                    if (updateCommentRequest.CommentText.Equals(comment.CommentText))
                        return response.GetResponse("No hay diferencia entre el comentario original y el nuevo igual se devuelve respuesta correcta", 1, true);

                    comment.CommentText = updateCommentRequest.CommentText;
                    db.Update(comment);
                    if (await db.SaveChangesAsync() == 1)
                        return response.GetResponse("Éxito al editar el comentario", 1, true);

                    return response.GetResponse("Ocuerrieron problemas al editar el comentario", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Hubo un error inesperado {e.Message}", 0, false);
            }
        }

        /// <summary>
        /// Eliminamos el comentario
        /// </summary>
        /// <param name="deleteCommentRequest">Modelo mapeado, esta vez preparado en el end point del controlador</param>
        /// <returns>Response</returns>
        public async Task<Response> DeleteComment(DeleteCommentRequest deleteCommentRequest)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    response = await ValidatorSecurityComment(deleteCommentRequest.NameUser, deleteCommentRequest.IdPost, deleteCommentRequest.IdComment);
                    if (response.Success.Equals(0))
                        return response;

                    Comment comment = await db.Comment.Where(c => c.Id.Equals(deleteCommentRequest.IdComment)).FirstOrDefaultAsync();
                    db.Comment.Remove(comment);

                    if (await db.SaveChangesAsync() == 1)
                        return response.GetResponse("Éxito al guardar el comentario", 1, true);

                    return response.GetResponse("Ocuerrieron problemas al eliminar el comentario", 0, false);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Hubo un error inesperado {e.Message}", 0, false);
            }
        }

        /// <summary>
        /// Codigo de seguridad que apoya a los metodos DeleteComment y UpdateComment
        /// </summary>
        /// <param name="nameUser">Nombre del usuario para verificar su existencia</param>
        /// <param name="idPost">Id del post para verificar su existencia</param>
        /// <param name="idComment">Id del comentario para verificar su existencia</param>
        /// <returns>Response</returns>
        public async Task<Response> ValidatorSecurityComment(string nameUser, int idPost, int idComment)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(nameUser)).FirstOrDefaultAsync();
                    if (user == null)
                        return response.GetResponse("El usuario no existe", 0, false);

                    Post post = await db.Post.Where(p => p.Id.Equals(idPost)).FirstOrDefaultAsync();
                    if (post == null)
                        return response.GetResponse("El post no existe", 0, false);

                    Comment comment = await db.Comment.Where(c => c.IdPost.Equals(idPost)
                    && c.Id.Equals(idComment)
                    && c.IdUser.Equals(user.Id)).FirstOrDefaultAsync();
                    if (comment == null)
                        return response.GetResponse("No existe el comentario en ese post", 0, false);

                    return response.GetResponse("Paso la validaciones correspondientes", 1, true);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Hubo un error inesperado {e.Message}", 0, false);
            }
        }
    }
}
