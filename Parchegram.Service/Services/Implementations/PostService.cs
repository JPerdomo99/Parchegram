﻿using Microsoft.AspNetCore.Http;
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
                    //IQueryable<PostListResponse> queryPosts = from logPost in db.LogPost
                    //                                          //join userLogPost in db.User on nameUser equals nameUser
                    //                                          join post in db.Post on logPost.IdPost equals post.Id into leftPost // Sacamos los post que coinciden con los registros de logPost
                    //                                          from subPost in leftPost.DefaultIfEmpty()
                    //                                          join userOwnerPost in db.User on subPost.IdUser equals userOwnerPost.Id into leftUserOwnerPost // Sacamos los dueños de los post
                    //                                          from subUserOwnerPost in leftUserOwnerPost.DefaultIfEmpty()
                    //                                          join share in db.Share on logPost.IdPost equals share.IdPost into leftShare // Sacamos los share que significa que usuarios que sigo han compartido post de otros
                    //                                          from subShare in leftShare.DefaultIfEmpty()
                    //                                          join userSharePost in db.User on subShare.IdUser equals userSharePost.Id into leftUserSharePost // Sacamos los usuarios que compartieron el post
                    //                                          from subUserSharePost in leftUserSharePost.DefaultIfEmpty()
                    //                                          where logPost.IdUserNavigation.NameUser == nameUser
                    //                                          orderby logPost.Date descending
                    //                                          select new PostListResponse
                    //                                          {
                    //                                              // Post
                    //                                              IdPost = subPost.Id,
                    //                                              IdTypePost = subPost.IdTypePost,
                    //                                              Description = subPost.Description,
                    //                                              PathFile = subPost.PathFile,
                    //                                              Date = subPost.Date,

                    //                                              // UserOwnerPost
                    //                                              IdUserOwnerPost = subUserOwnerPost.Id,
                    //                                              NameUserOwnerPost = subUserOwnerPost.NameUser,

                    //                                              // UserSharePost
                    //                                              IdUserSharePost = subUserSharePost.Id,
                    //                                              NameUserUserSharePost = subUserSharePost.NameUser
                    //                                          };

                    #region
                    var followTest = db.Follow.Where(f => f.IdUserFollower.Equals(1)).Select(f => f.IdUserFollowing);
                    var test = from tempPost in db.Post

                               join tempUserOwner in db.User on tempPost.IdUser equals tempUserOwner.Id

                               join tempShare in db.Share on tempPost.Id equals tempShare.IdPost into leftTempShare
                               from subTempShare in leftTempShare.DefaultIfEmpty()

                               join tempFollow in db.Follow on tempPost.IdUser equals tempFollow.IdUserFollowing into leftTempFollow
                               from subTempFollow in leftTempFollow.DefaultIfEmpty()

                               join TempUserShare in db.User on subTempShare.IdUser equals TempUserShare.Id into leftTempUserShare
                               from subTempUserShare in leftTempUserShare.DefaultIfEmpty()

                               where subTempFollow.IdUserFollower.Equals(1) || followTest.Contains(subTempShare.IdUser)
                               //subTempShare.IdUser.Equals(db.Follow.Where(f => f.IdUserFollower.Equals(1)).Select(f => f.IdUserFollowing))

                               select new { tempPost, tempUserOwner, subTempShare, subTempFollow, subTempUserShare };
                    #endregion

                    foreach (var item in test)
                    {
                        _logger.LogInformation(item.ToString());
                    }
                    
                    ICollection < PostListResponse > listPosts = new List<PostListResponse>();
                    ILikeService likeService = new LikeService();
                    ICommentService commentService = new CommentService();
                    //foreach (var post in queryPosts)
                    //{
                    //    post.NumLikes = likeService.GetNumLikes(post.IdPost);
                    //    post.CommentsByPost = commentService.GetCommentsByPost(post.IdPost, false);
                    //    if (post.PathFile != null)
                    //        post.File = GetFile(post.PathFile);
                    //    listPosts.Add(post);
                    //}

                    return listPosts;
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
