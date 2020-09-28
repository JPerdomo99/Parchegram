using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.Post;
using Parchegram.Service.ClassesSupport;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Service.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Implementations
{
    public class PostService : IPostService
    {
        public struct Paginate
        {
            public int totalRecords, recordsPerPage;

            public Paginate(int totalRecords, int recordsPerPage)
            {
                this.totalRecords = totalRecords;
                this.recordsPerPage = recordsPerPage;
            }
        }

        private readonly ILogger _logger;

        public PostService()
        {
        }

        public PostService(ILogger<PostService> logger)
        {
            _logger = logger;
        }

        public async Task<Response> CreatePost(CreatePostRequest createPostRequest)
        {
            Response response = new Response();
            // Validamos el archivo (extension y tamaño)
            if (createPostRequest.File != null)
            {
                if (!ValidateFile.ValidateExtensionFile(createPostRequest.File.ContentType))
                {
                    return response.GetResponse($"Formato de archivo no valido {createPostRequest.File.ContentType}",
                                                0, false);
                }
                if (!ValidateFile.ValidateSizeFile(createPostRequest.File.Length, 8000000))
                {
                    return response.GetResponse($"Máximo 8MB para el archivo: {ValidateFile.ConvertToMegabytes(createPostRequest.File.Length)}",
                                                0, false);
                }
            }

            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = db.User.Where(u => u.NameUser == createPostRequest.NameUser).FirstOrDefault();
                    if (user != null)
                    {
                        int idUser = await db.User.Where(u => u.NameUser == createPostRequest.NameUser).Select(u => u.Id).FirstOrDefaultAsync();
                        Post post = new Post();
                        post.Description = createPostRequest.Description;
                        post.IdUser = idUser;
                        post.IdTypePost = DefineTypePost(createPostRequest.File);
                        //post.IdTypePost = createPostRequest.IdTypePost;
                        post.Date = DateTime.Now;
                        if (createPostRequest.IdTypePost != 3)
                        {
                            string pathFile = GetPathFile(createPostRequest.File);
                            post.PathFile = pathFile;
                            SaveFile(createPostRequest.File);
                        }

                        db.Post.Add(post);
                        db.SaveChanges();
                    }
                    else
                    {
                        return response.GetResponse("El usuario no existe", 0, false);
                    }
                    return response.GetResponse("Post creado correctamente", 1, true);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return response.GetResponse($"Ha ocurrido un error {e.Message}", 0, false);
                }
            }
        }

        public bool DeletePost(int id)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post post = db.Post.Where(p => p.Id == id).FirstOrDefault();
                    db.Remove(post);

                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        public bool EditPost(EditPostRequest editPostRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post post = db.Post.Where(p => p.Id == editPostRequest.Id).FirstOrDefault();
                    if (post == null)
                        return false;

                    post.Description = editPostRequest.Description;
                    post.IdTypePost = editPostRequest.IdTypePost;
                    post.PathFile = editPostRequest.PathFile;

                    db.Post.Update(post);
                    if (db.SaveChanges() == 1)
                        return true;

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
                }
            }
        }

        public PostResponse GetPostById(int id)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    //PostResponse oPost =
                    //                        (from post in db.Post
                    //                         join user in db.User on post.IdUser equals user.Id
                    //                         where post.IdUser == id
                    //                         select new PostResponse
                    //                         {
                    //                             Id = post.Id,
                    //                             Description = post.Description,
                    //                             PathFile = post.PathFile,
                    //                             Date = post.Date,
                    //                             IdTypePost = post.IdTypePost,
                    //                             IdUser = post.IdUser,
                    //                             NameUser = user.NameUser
                    //                         }).FirstOrDefault();

                    //if (oPost == null)
                    //    return null;

                    //ILikeService oLikeService = new LikeService();
                    //ICommentService oCommentService = new CommentService();
                    //oPost.NumLikes = oLikeService.GetNumLikes(oPost.Id);
                    //oPost.CommentsByPost = oCommentService.GetCommentsByPost(oPost.Id, true);

                    //return oPost;
                    return null;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Devuelve un response con todos los post de los usuarios que sigo
        /// Y los que los usuarios que siguen compartieron
        /// </summary>
        /// <param name="nameUser">Nombre del usuario</param>
        /// <returns>Response con los posts cada uno con su numero de likes y comentarios limit(2)</returns>
        public async Task<Response> GetPostList(string nameUser, int page = 1)
        {
            Response response = new Response();
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(nameUser)).FirstOrDefaultAsync();
                    if (user == null)
                        return response.GetResponse("No existe un usuario con ese nombre", 0, null);

                    PostListPaginateResponse postListPaginateResponse = await GetPostListPaginateResponse(user.Id, page, db);

                    return response.GetResponse("Exito al consultar los post", 1, postListPaginateResponse);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return response.GetResponse($"Ha ocurrido un error inesperado {e.Message}", 0, null);
                }
            }
        }

        /// <summary>
        /// Recorre el resultado de la consulta y almacena cada uno en una lista
        /// con sus respectivo número de likes y comentarios limmite 2
        /// </summary>
        /// <param name="idUser">Id del usuario</param>
        /// <param name="db">Contexto de la db</param>
        /// <returns>Collection de PostResponse</returns>
        private async Task<PostListPaginateResponse> GetPostListPaginateResponse(int idUser, int page, ParchegramDBContext db)
        {
            IQueryable<PostListQueryResponse> queryPostResponses = GetPostsQueryResponses(idUser, db);
            ICollection<PostResponse> postResponsesCollection = await ProcessPosts(queryPostResponses);
            IOrderedEnumerable<PostResponse> sortedPostResponses = GetPostsOrder(postResponsesCollection);
            IImmutableList<PostResponse> postResponses = GetPostspaginate(sortedPostResponses, page);
            int totalRows = sortedPostResponses.Count();
            PostListPaginateResponse postListPaginateResponse = new PostListPaginateResponse(postResponses, totalRows);
            
            return postListPaginateResponse;
        }

        /// <summary>
        /// Ejecuta consulta saca todos los post con uniones y restricciones y devuelve el resultado
        /// </summary>
        /// <param name="idUser">Id del usuario al cual le vamos a mostrar los posts</param>
        /// <returns>Devuelve la consulta que contiene los post que se devolveran al cliente correspondiente</returns>
        private IQueryable<PostListQueryResponse> GetPostsQueryResponses(int idUser, ParchegramDBContext parchegramDBContext)
        {
            try
            {
                IQueryable<PostListQueryResponse> queryPostResponses = (from tempPost in parchegramDBContext.Post

                                                                        join tempUserOwner in parchegramDBContext.User on tempPost.IdUser equals tempUserOwner.Id

                                                                        join tempShare in parchegramDBContext.Share on tempPost.Id equals tempShare.IdPost into leftTempShare
                                                                        from subTempShare in leftTempShare.DefaultIfEmpty()

                                                                        join tempFollow in parchegramDBContext.Follow on tempPost.IdUser equals tempFollow.IdUserFollowing into leftTempFollow
                                                                        from subTempFollow in leftTempFollow.DefaultIfEmpty()

                                                                        join tempUserShare in parchegramDBContext.User on subTempShare.IdUser equals tempUserShare.Id into leftTempUserShare
                                                                        from subTempUserShare in leftTempUserShare.DefaultIfEmpty()

                                                                        join tempLike in parchegramDBContext.Like on tempPost.Id equals tempLike.IdPost into leftTempLike
                                                                        from subTempLike in leftTempLike.DefaultIfEmpty()

                                                                            // Si el que compartio el post es seguido por el usuario 1
                                                                        where subTempFollow.IdUserFollower.Equals(1) ||
                                                                        parchegramDBContext.Follow.Any(f => f.IdUserFollower.Equals(1) && f.IdUserFollowing.Equals(subTempShare.IdUser)) ||
                                                                        subTempLike.IdUser.Equals(1)

                                                                        select new PostListQueryResponse
                                                                        {
                                                                            QueryPost = tempPost,
                                                                            QueryUserOwner = tempUserOwner,
                                                                            QueryShare = subTempShare,
                                                                            QueryFollow = subTempFollow,
                                                                              QueryUserShare = subTempUserShare,
                                                                            QueryLike = subTempLike
                                                                        }); 
                return queryPostResponses;
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Recorre la consulta asignando fecha subido/compartido, comentarios y numero de likes
        /// </summary>
        /// <param name="postListQueryResponses">Query para recorrer</param>
        /// <returns>Colección </returns>
        private async Task<ICollection<PostResponse>> ProcessPosts(IQueryable<PostListQueryResponse> postListQueryResponses)
        {
            ICollection<PostResponse> postResponses = new List<PostResponse>();
            ImageUserProfile imageUserProfile = new ImageUserProfile(false);
            ILikeService likeService = new LikeService();
            foreach (var post in postListQueryResponses)
            {
                PostResponse postResponse = new PostResponse();

                // Post
                postResponse.IdPost = post.QueryPost.Id;
                postResponse.IdTypePost = post.QueryPost.IdTypePost;
                postResponse.Description = post.QueryPost.Description;
                // El post fue compartido entonces asignamos la fecha del share
                postResponse.Date = (post.QueryFollow != null) ? post.QueryPost.Date : post.QueryShare.Date;

                if (post.QueryPost.PathFile != null)
                    postResponse.File = await Image.GetFile(post.QueryPost.PathFile);

                // UserOwner
                postResponse.IdUserOwner = post.QueryUserOwner.Id;
                postResponse.NameUserOwner = post.QueryUserOwner.NameUser;
                postResponse.ImageProfileUserOwner = await imageUserProfile.GetImageUser(post.QueryUserOwner.Id, 'S');

                // UserShare
                if (post.QueryShare != null)
                {
                    postResponse.IdUserShare = post.QueryUserShare.Id;
                    postResponse.NameUserShare = post.QueryUserShare.NameUser;
                    postResponse.ImageProfileUserShare = await imageUserProfile.GetImageUser(post.QueryUserShare.Id, 'S');
                }

                postResponse.LikeUser = (post.QueryLike != null) ? true : false;

                // Numero de likes de la publicación
                postResponse.NumberLikes = likeService.GetNumLikes(post.QueryPost.Id);

                postResponses.Add(postResponse);
            }
            return postResponses;
        }

        /// <summary>
        /// Ordena la colección de post por fecha de forma desendente
        /// </summary>
        /// <param name="postResponsesCollection">Colección de post</param>
        /// <returns>Lista enumarada de PostResponse ordenada por fecha</returns>
        private IOrderedEnumerable<PostResponse> GetPostsOrder(ICollection<PostResponse> postResponsesCollection)
        {
            return postResponsesCollection.OrderByDescending(p => p.Date);
        }

        /// <summary>
        /// Pagina los post devolviendolos en una lista inmutable
        /// </summary>
        /// <param name="sortedPostResponses">PostResponse enumarados y ordenados</param>
        /// <param name="page">Pagina de la queremos traer los post</param>
        /// <returns>Lista inmutable de PostResponse</returns>
        private IImmutableList<PostResponse> GetPostspaginate(IOrderedEnumerable<PostResponse> sortedPostResponses, int page)
        {
            return sortedPostResponses.Skip((page - 1) * 3).Take(3).ToImmutableList();
        }

        /// <summary>
        /// Obtiene path del archivo que llega por post
        /// </summary>
        /// <param name="file">Archivo que llega desde un formulario</param>
        /// <returns>Ruta de donde se guardara el archivo</returns>
        private string GetPathFile(IFormFile file)
        {
            try
            {
                string rootPath = @"C:\Media\Post";
                string fileName = file.FileName;

                return Path.Combine(rootPath, fileName);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Copia el archivo en la ruta especificada
        /// </summary>
        /// <param name="file">Archivo que llega desde un formulario</param>
        private void SaveFile(IFormFile file)
        {
            try
            {
                using (var fs = File.Create(GetPathFile(file)))
                {
                    file.CopyTo(fs);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        /// <summary>
        /// Metodo que define el tipo tipo de post dependiendo si hay o no archivo
        /// </summary>
        /// <param name="formFile">Archivo que llegas desde un form imagen o video</param>
        /// <returns>Id del tipo de archivo del 1 al 3</returns>
        private int DefineTypePost(IFormFile formFile)
        {
            return (formFile != null) ? DefineTypePostFile(formFile.ContentType) : 3;
        }

        /// <summary>
        /// Define el tipo de archivo si es imagen o video
        /// </summary>
        /// <param name="extension">Extension del archivo</param>
        /// <returns>Tipo del archivo 1 si es imagen 2 si es video</returns>
        private int DefineTypePostFile(string extension)
        {
            if (extension.Contains("image"))
                return 1;
            else if (extension.Contains("video"))
                return 2;
            else
                return 0;
        }
    }
}
