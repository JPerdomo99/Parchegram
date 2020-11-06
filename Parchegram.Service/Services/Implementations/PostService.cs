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
                        Post post = new Post();
                        post.Description = createPostRequest.Description;
                        post.IdUser = user.Id;
                        post.IdTypePost = DefineTypePost(createPostRequest.File);
                        post.Date = DateTime.Now;
                        if (createPostRequest.IdTypePost != 3)
                        {
                            FileStocker fileStocker = new FileStocker(createPostRequest.File);
                            post.PathFile = fileStocker.FullPath;
                            await fileStocker.SaveFile(); 
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

        /// <summary>
        /// Obtenemos un post segun su id
        /// </summary>
        /// <param name="id">Id del post</param>
        /// <returns>Post</returns>
        public async Task<Response> GetPostById(int id)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    Post post = await db.Post.Where(p => p.Id.Equals(id)).FirstOrDefaultAsync();
                    if (post == null)
                        return response.GetResponse("No existe el post", 0, null);
                    PostListQueryResponse postListQueryResponse = await GetPostsQueryResponseById(id, db);
                    PostResponse postRensponse = await ProcessPostById(postListQueryResponse);

                    return response.GetResponse($"Exito al traer el post", 1, postRensponse);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado {e.Message}", 0, null);
            }
        }

        /// <summary>
        /// Devuelve un response con todos los post de los usuarios que sigo
        /// Y los que los usuarios que siguen compartieron
        /// </summary>
        /// <param name="nameUser">Nombre del usuario</param>
        /// <param name="page">Pagina que de la que vamos a consultar los post</param>
        /// <returns>Response con los posts cada uno con su numero de likes y comentarios limit(2)</returns>
        public async Task<Response> GetPostList(string nameUser, int page)
        {
            Response response = new Response();
            try { 
                using (var db = new ParchegramDBContext())
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(nameUser)).FirstOrDefaultAsync();
                    if (user == null)
                        return response.GetResponse("No existe un usuario con ese nombre", 0, null);

                    PostListPaginateResponse postListPaginateResponse = await GetPostListPaginateResponse(user.Id, page, db);

                    return response.GetResponse("Exito al consultar los post", 1, postListPaginateResponse);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado {e.Message}", 0, null);
            }
        }

        /// <summary>
        /// Devuelve un response con todos los post que ha subido o compartido segun su id
        /// </summary>
        /// <param name="idUser">Id del usuario del que vamos a consultar los post</param>
        /// <param name="page">Pagina </param>
        /// <param name="idTypePost"></param>
        /// <param name="nameUserSession"></param>
        /// <returns>Devuelve un response con todos los post que ha subido o compartido segun su id</returns>
        public async Task<Response> GetPostList(string nameUser, int page, int idTypePost, string nameUserSession)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User user = await db.User.Where(u => u.NameUser.Equals(nameUser)).FirstOrDefaultAsync();
                    if (user == null)
                        return response.GetResponse("No existe un usuario con ese nombre", 0, null);
                    User userSession = await db.User.Where(u => u.NameUser.Equals(nameUserSession)).FirstOrDefaultAsync();
                    if (userSession == null)
                        return response.GetResponse("No existe el usuario de la sesión", 0, null);

                    PostListPaginateResponse postListPaginateResponse = await GetPostListPaginateResponse(user.Id, page, db, idTypePost, userSession.Id);

                    return response.GetResponse("Exito al consultar los post", 1, postListPaginateResponse);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado {e.Message}", 0, null);
            }
        }

        /// <summary>
        /// Recorre el resultado de la consulta y almacena cada uno en una lista
        /// con sus respectivo número de likes y comentarios limmite 2
        /// </summary>
        /// <param name="idUser">Id del usuario</param>
        /// <param name="db">Contexto de la db</param>
        /// <returns>Collection de PostResponse</returns>
        private async Task<PostListPaginateResponse> GetPostListPaginateResponse(int idUser, int page, ParchegramDBContext db, int idTypePost = 0, int idUserSession = 0)
        {
            IQueryable<PostListQueryResponse> queryPostResponses = null;
            if (idUserSession != 0)
                queryPostResponses = GetPostsQueryResponses(idUser, db, idTypePost, idUserSession);
            else
                queryPostResponses = GetPostsQueryResponses(idUser, db);
            ICollection<PostResponse> postResponsesCollection = await ProcessPosts(queryPostResponses);
            IOrderedEnumerable<PostResponse> sortedPostResponses = GetPostsOrder(postResponsesCollection);
            IImmutableList<PostResponse> postResponses = GetPostspaginate(sortedPostResponses, page);
            int totalRows = sortedPostResponses.Count();
            PostListPaginateResponse postListPaginateResponse = new PostListPaginateResponse(postResponses, totalRows);

            return postListPaginateResponse;
        }

        /// <summary>
        /// Obtiene un Post segun su Id devolviendo el Post, el usuario que la subio y si se ha dado like
        /// </summary>
        /// <param name="idPost">Id del post</param>
        /// <param name="parchegramDBContext">Contexto de la base de datos</param>
        /// <returns>El post junto con el usario que lo subio y si se ha dado like</returns>
        private async Task<PostListQueryResponse> GetPostsQueryResponseById(int idPost, ParchegramDBContext parchegramDBContext)
        {
            PostListQueryResponse queryPostResponses = await (from tempPost in parchegramDBContext.Post

                                                              join tempUserOwner in parchegramDBContext.User on tempPost.IdUser equals tempUserOwner.Id

                                                              join tempLike in parchegramDBContext.Like on tempPost.Id equals tempLike.IdPost into leftTempLike
                                                              from subTempLike in leftTempLike.DefaultIfEmpty()

                                                              where tempPost.Id.Equals(idPost)

                                                              select new PostListQueryResponse
                                                              {
                                                                  QueryPost = tempPost,
                                                                  QueryUserOwner = tempUserOwner,
                                                                  QueryLike = subTempLike
                                                              }).FirstOrDefaultAsync();
            return queryPostResponses;
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
                                                                        where subTempFollow.IdUserFollower.Equals(idUser) ||
                                                                        parchegramDBContext.Follow.Any(f => f.IdUserFollower.Equals(idUser) && f.IdUserFollowing.Equals(subTempShare.IdUser)) ||
                                                                        subTempLike.IdUser.Equals(idUser)

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
        /// Obtiene la lista de los post que ha subido o ha compartido un usuario segun su id
        /// </summary>
        /// <param name="idUser">Id del usuario del que vamos a consultar los post</param>
        /// <param name="parchegramDBContext">Contexto de la base de datos</param>
        /// <param name="idTypePost">Si solo vamos a consultar por un tipo de post</param>
        /// <param name="idUserSession">Id del usuario que esta en la sesión con la finalidad de consultar su id</param>
        /// <returns>Devuelve la consulta que contiene los post que se devolveran al cliente correspondiente</returns>
        private IQueryable<PostListQueryResponse> GetPostsQueryResponses(int idUser, ParchegramDBContext parchegramDBContext, int idTypePost, int idUserSession)
        {
            IQueryable<PostListQueryResponse> queryPostResponses = (from tempPost in parchegramDBContext.Post

                                                                    join tempUserOwner in parchegramDBContext.User on tempPost.IdUser equals tempUserOwner.Id into leftTempUserOwner
                                                                    from subTempUserOwner in leftTempUserOwner.DefaultIfEmpty()

                                                                    join tempShare in parchegramDBContext.Share on tempPost.Id equals tempShare.IdPost into leftTempShare
                                                                    from subTempShare in leftTempShare.DefaultIfEmpty()

                                                                    join tempUserShare in parchegramDBContext.User on subTempShare.IdUser equals tempUserShare.Id into leftTempUserShare
                                                                    from subTempUserShare in leftTempUserShare.DefaultIfEmpty()

                                                                    join tempLike in parchegramDBContext.Like.Where(l => l.IdUser.Equals(idUserSession)) on tempPost.Id equals tempLike.IdPost into leftTempLike
                                                                    from subTempLike in leftTempLike.DefaultIfEmpty()

                                                                    where tempPost.IdUser.Equals(idUser) || subTempUserShare.Id.Equals(idUser)

                                                                    select new PostListQueryResponse
                                                                    {
                                                                        QueryPost = tempPost,
                                                                        QueryUserOwner = subTempUserOwner,
                                                                        QueryShare = subTempShare,
                                                                        QueryUserShare = subTempUserShare,
                                                                        QueryLike = subTempLike
                                                                    });
            if (idTypePost != 0)
                queryPostResponses = queryPostResponses.Where(q => q.QueryPost.IdTypePost.Equals(idTypePost));
            return queryPostResponses;
        }

        /// <summary>
        /// Se procesa un solo post
        /// </summary>
        /// <param name="postListQueryResponse">En este caso ya devolvemos el post ya que se ejecuta la consulta</param>
        /// <param name="byId">Id del post que quereoms consultar</param>
        /// <returns></returns>
        private async Task<PostResponse> ProcessPostById(PostListQueryResponse postListQueryResponse)
        {
            try
            {
                PostResponse postResponse = new PostResponse();
                ImageUserProfile imageUserProfile = new ImageUserProfile(false);
                ILikeService likeService = new LikeService();

                // Post
                postResponse.IdPost = postListQueryResponse.QueryPost.Id;
                postResponse.IdTypePost = postListQueryResponse.QueryPost.IdTypePost;
                postResponse.Description = postListQueryResponse.QueryPost.Description;
                postResponse.Date = postListQueryResponse.QueryPost.Date;
                if (postListQueryResponse.QueryPost.PathFile != null)
                    postResponse.File = await Image.GetFile(postListQueryResponse.QueryPost.PathFile);

                // UserOwner
                postResponse.IdUserOwner = postListQueryResponse.QueryUserOwner.Id;
                postResponse.NameUserOwner = postListQueryResponse.QueryUserOwner.NameUser;
                postResponse.ImageProfileUserOwner = await imageUserProfile.GetImageUser(postListQueryResponse.QueryUserOwner.Id, 'S');

                postResponse.LikeUser = (postListQueryResponse.QueryLike != null) ? true : false;

                // Numero de likes de la publicación
                postResponse.NumberLikes = likeService.GetNumLikes(postListQueryResponse.QueryPost.Id);

                return postResponse;
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
        /// <returns>Colección tipo PostResponse</returns>
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
                // Eso en caso de que consultemos los follows, este caso se presenta
                // cuando se consultas los post desde la pagina Home
                postResponse.Date = (post.QueryFollow != null) ? post.QueryPost.Date : ((post.QueryShare != null) ? post.QueryShare.Date : post.QueryPost.Date);

                if (post.QueryPost.PathFile != null)
                {
                    if (post.QueryPost.IdTypePost.Equals(1))
                        postResponse.File = await Image.GetFile(post.QueryPost.PathFile);
                    else if (post.QueryPost.IdTypePost.Equals(2))
                        postResponse.File = await Image.GetFile(post.QueryPost.PathFile);
                }

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
                    postResponse.DateShare = post.QueryShare.Date;
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
