using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.Post;
using Parchegram.Service.Services.Interfaces;
using Parchegram.Service.Tools;
using System;
using System.Collections.Generic;
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
                    } else
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
                    Post oPost = db.Post.Where(p => p.Id == id).FirstOrDefault();
                    db.Remove(oPost);


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
                    Post oPost = db.Post.Where(p => p.Id == editPostRequest.Id).FirstOrDefault();
                    if (oPost == null)
                        return false;

                    oPost.Description = editPostRequest.Description;
                    oPost.IdTypePost = editPostRequest.IdTypePost;
                    oPost.PathFile = editPostRequest.PathFile;

                    db.Post.Update(oPost);
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
                    PostResponse oPost =
                                            (from post in db.Post
                                             join user in db.User on post.IdUser equals user.Id
                                             where post.IdUser == id
                                             select new PostResponse
                                             {
                                                 Id = post.Id,
                                                 Description = post.Description,
                                                 PathFile = post.PathFile,
                                                 Date = post.Date,
                                                 IdTypePost = post.IdTypePost,
                                                 IdUser = post.IdUser,
                                                 NameUser = user.NameUser
                                             }).FirstOrDefault();

                    if (oPost == null)
                        return null;

                    ILikeService oLikeService = new LikeService();
                    ICommentService oCommentService = new CommentService();
                    oPost.NumLikes = oLikeService.GetNumLikes(oPost.Id);
                    oPost.CommentsByPost = oCommentService.GetCommentsByPost(oPost.Id, true);

                    return oPost;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

        public ICollection<PostListResponse> GetPostList(string nameUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    IQueryable<PostListQueryResponse> queryResult = GetPostListQueryResponses(1);
                    ICollection<PostResponse> listPosts = new List<PostResponse>();
                    ILikeService likeService = new LikeService();
                    ICommentService commentService = new CommentService();
                    foreach (var post in queryResult)
                    {
                        PostResponse postResponse = new PostResponse();

                        // Post
                        postResponse.IdPost = post.QueryPost.Id;
                        postResponse.IdTypePost = post.QueryPost.IdTypePost;
                        postResponse.Description = post.QueryPost.Description;
                        // El post fue compartido entonces asignamos la fecha del share
                        if (post.QueryFollow.Equals(null))
                            postResponse.Date = post.QueryShare.Date;
                        else
                            postResponse.Date = post.QueryPost.Date;

                        if (post.QueryPost.PathFile != null)
                            postResponse.File = GetFile(post.QueryPost.PathFile);


                        if (item.subTempFollow.Equals(null))
                            postResponseTest.Date = item.subTempShare.Date;
                        else
                            postResponseTest.Date = item.tempPost.Date;

                        postResponseTest.IdUserFollowing = item.subTempFollow.IdUserFollowing;
                        // Imagen del usuario que subio el post en bytes

                        if (item.subTempShare != null)
                        {
                            postResponseTest.IdUserShare = item.subTempShare.IdUser;
                            // Nombre del usuario
                            // Imagen del usuario que compartio el post en bytes
                        }

                        post.NumLikes = likeService.GetNumLikes(post.IdPost);
                        post.CommentsByPost = commentService.GetCommentsByPost(post.IdPost, false);
                        if (post.PathFile != null)
                            post.File = GetFile(post.PathFile);
                        listPosts.Add(post);
                    }

                    return listPosts;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Ejecuta consulta de posts con uniones y devuelve el resultado
        /// </summary>
        /// <param name="idUser">Id del usuario al cual le vamos a mostrar los posts</param>
        /// <returns>Devuelve la consulta que contiene los post que se devolveran al cliente correspondiente</returns>
        private IQueryable<PostListQueryResponse> GetPostListQueryResponses(int idUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    IQueryable<PostListQueryResponse> query = from tempPost in db.Post

                                                             join tempUserOwner in db.User on tempPost.IdUser equals tempUserOwner.Id

                                                             join tempShare in db.Share on tempPost.Id equals tempShare.IdPost into leftTempShare
                                                             from subTempShare in leftTempShare.DefaultIfEmpty()

                                                             join tempFollow in db.Follow on tempPost.IdUser equals tempFollow.IdUserFollowing into leftTempFollow
                                                             from subTempFollow in leftTempFollow.DefaultIfEmpty()

                                                             join TempUserShare in db.User on subTempShare.IdUser equals TempUserShare.Id into leftTempUserShare
                                                             from subTempUserShare in leftTempUserShare.DefaultIfEmpty()
                                                                                                             // Si el que compartio el post es seguido por el usuario 1
                                                             where subTempFollow.IdUserFollower.Equals(1) || db.Follow.Any(f => f.IdUserFollower.Equals(1) && f.IdUserFollowing.Equals(subTempShare.IdUser))

                                                             select new PostListQueryResponse
                                                             {
                                                                 QueryPost = tempPost,
                                                                 QueryUserOwner = tempUserOwner,
                                                                 QueryShare = subTempShare,
                                                                 QueryFollow = subTempFollow,
                                                                 QueryUserShare = subTempUserShare
                                                             };
                    return query;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

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
                return "";
            }
        }

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

        private byte[] GetFile(string fullPath)
        {
            byte[] result;
            using (FileStream file = File.Open(fullPath, FileMode.Open))
            {
                result = new byte[file.Length];
                file.ReadAsync(result, 0, (int)file.Length);
            }

            return result;
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
