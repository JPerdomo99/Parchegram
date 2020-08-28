using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Comment;
using Parchegram.Model.Response.Comment;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public ICollection<PostCommentResponse> GetCommentsByPost(int idPost, bool byId)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    ICollection<PostCommentResponse> commentsByPost = null;
                    if (byId)
                    {
                        commentsByPost =
                                            (from comment in db.Comment
                                             join user in db.User on comment.IdUser equals user.Id
                                             where comment.IdPost == idPost
                                             orderby comment.Date
                                             select new PostCommentResponse
                                             {
                                                 NameUser = user.NameUser,
                                                 IdUser = comment.IdUser,
                                                 CommentText = comment.CommentText,
                                                 Date = comment.Date
                                             }).ToList(); ;
                    }
                    else
                    {
                        commentsByPost =
                                            (from comment in db.Comment
                                             join user in db.User on comment.IdUser equals user.Id
                                             where comment.IdPost == idPost
                                             orderby comment.Date
                                             select new PostCommentResponse
                                             {
                                                 NameUser = user.NameUser,
                                                 IdUser = comment.IdUser,
                                                 CommentText = comment.CommentText,
                                                 Date = comment.Date
                                             }).Take(2).ToList();
                    }

                    return commentsByPost;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return null;
                }
            }
        }

        public bool PostComment(PostCommentRequest postCommentRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Post oPost = db.Post.Where(p => p.Id == postCommentRequest.IdPost).FirstOrDefault();
                    if (oPost == null)
                        return false;

                    User oUser = db.User.Where(u => u.Id == postCommentRequest.IdUser).FirstOrDefault();
                    if (oUser == null)
                        return false;

                    Comment oComment = new Comment();
                    oComment.IdUser = postCommentRequest.IdUser;
                    oComment.IdPost = postCommentRequest.IdPost;
                    oComment.CommentText = postCommentRequest.CommentText;
                    oComment.Date = DateTime.Now;

                    db.Comment.Add(oComment);
                    if (db.SaveChanges() == 1)
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    return false;
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
