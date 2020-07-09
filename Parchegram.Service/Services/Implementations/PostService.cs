using Parchegram.Model.Models;
using Parchegram.Model.Post.Request;
using Parchegram.Model.Request.Post;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using Parchegram.Model.Response.Post;
using Parchegram.Model.Request.Comment;
using Parchegram.Model.Response.Comment;
using Microsoft.Data.SqlClient;
using Parchegram.Model.Request.Like;

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
                        oPost.PathFile = createPostRequest.PathFile;

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
                                                NameUser= user.NameUser
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

        public ICollection<PostResponse> GetPostList(int idUser)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User oUser = db.User.Where(u => u.Id == idUser).FirstOrDefault();
                    IQueryable<PostResponse> queryPosts =           // Traer los post de los usuarios que sigue un usuario y los compartidos
                                                                    from post in db.Post
                                                                    join follow in db.Follow on oUser equals follow.IdUserFollowerNavigation
                                                                    join user in db.User on follow.IdUserFollower equals user.Id
                                                                    join share in db.Share on follow.IdUserFollowing equals share.IdUser
                                                                    where post.IdUser == follow.IdUserFollower && post.IdUser == share.IdUser
                                                                    orderby post.Date
                                                                    select new PostResponse
                                                                    {
                                                                        Id = post.Id,
                                                                        Description = post.Description,
                                                                        PathFile = post.PathFile,
                                                                        Date = post.Date,
                                                                        IdTypePost = post.IdTypePost,   
                                                                        IdUser = post.IdUser,
                                                                        NameUser = user.NameUser
                                                                    };

                    ICollection<PostResponse> lstPost = null;
                    ILikeService oLikeService = new LikeService();
                    ICommentService oCommentService = new CommentService(); 
                    foreach (PostResponse post in queryPosts)
                    {
                        post.NumLikes = oLikeService.GetNumLikes(post.Id);
                        post.CommentsByPost = oCommentService.GetCommentsByPost(post.Id, false);
                        lstPost.Add(post);
                    }
                    
                    return lstPost;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                } 
            }
        }
    }
}
