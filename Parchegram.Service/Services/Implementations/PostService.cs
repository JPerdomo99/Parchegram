using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Post;
using Parchegram.Model.Response;
using Parchegram.Model.Response.Post;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public bool CreatePost(CreatePostRequest createPostRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post oPost = new Post();
                    oPost.Description = createPostRequest.Description;
                    oPost.IdUser = createPostRequest.IdUser;
                    oPost.IdTypePost = createPostRequest.IdTypePost;
                    oPost.Date = DateTime.Now;
                    if (createPostRequest.IdTypePost != 3)
                    {
                        string pathFile = GetPathFile(createPostRequest.File);
                        oPost.PathFile = pathFile;
                        SaveFile(createPostRequest.File);
                    }

                    db.Post.Add(oPost);
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
                    IQueryable<PostListResponse> queryPosts = from logPost in db.LogPost
                                                                  //join userLogPost in db.User on nameUser equals nameUser
                                                              join post in db.Post on logPost.IdPost equals post.Id into leftPost // Sacamos los post que coinciden con los registros de logPost
                                                              from subPost in leftPost.DefaultIfEmpty()
                                                              join userOwnerPost in db.User on subPost.IdUser equals userOwnerPost.Id into leftUserOwnerPost // Sacamos los dueños de los post
                                                              from subUserOwnerPost in leftUserOwnerPost.DefaultIfEmpty()
                                                              join share in db.Share on logPost.IdPost equals share.IdPost into leftShare // Sacamos los share que significa que usuarios que sigo han compartido post de otros
                                                              from subShare in leftShare.DefaultIfEmpty()
                                                              join userSharePost in db.User on subShare.IdUser equals userSharePost.Id into leftUserSharePost // Sacamos los usuarios que compartieron el post
                                                              from subUserSharePost in leftUserSharePost.DefaultIfEmpty()
                                                              where logPost.IdUserNavigation.NameUser == nameUser
                                                              orderby logPost.Date descending
                                                              select new PostListResponse
                                                              {
                                                                  // Post
                                                                  IdPost = subPost.Id,
                                                                  IdTypePost = subPost.IdTypePost,
                                                                  Description = subPost.Description,
                                                                  PathFile = subPost.PathFile,
                                                                  Date = subPost.Date,

                                                                  // UserOwnerPost
                                                                  IdUserOwnerPost = subUserOwnerPost.Id,
                                                                  NameUserOwnerPost = subUserOwnerPost.NameUser,

                                                                  // UserSharePost
                                                                  IdUserSharePost = subUserSharePost.Id,
                                                                  NameUserUserSharePost = subUserSharePost.NameUser
                                                              };

                    ICollection<PostListResponse> listPosts = new List<PostListResponse>();
                    ILikeService likeService = new LikeService();
                    ICommentService commentService = new CommentService();
                    foreach (var post in queryPosts)
                    {
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
    }
}
