using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Request.Follow;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace Parchegram.Service.Services.Implementations
{
    public class FollowService : IFollowService
    {
        private readonly ILogger _logger;

        public FollowService(ILogger logger)
        {
            _logger = logger;
        }

        public bool AddFollow(FollowRequest followRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {

                    Follow follow = GetFollow(followRequest);

                    if (follow == null)
                    {
                        Follow oFollow = new Follow();
                        oFollow.IdUserFollower = followRequest.IdUserFollower;
                        oFollow.IdUserFollowing = followRequest.IdUserFollowing;

                        db.Follow.Add(oFollow);
                        if (db.SaveChanges() == 1)
                            return true;

                        return false;
                    }

                    // El follow ya existe
                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                     return false;
                }
            }
        }

        public bool RemoveFollow(FollowRequest followRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Follow follow = GetFollow(followRequest);

                    if (follow == null)
                        return false;

                    db.Follow.Remove(follow);
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

        private Follow GetFollow(FollowRequest followRequest)
        {
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    Follow follow = db.Follow.Where(f => f.IdUserFollower == followRequest.IdUserFollower &&
                            f.IdUserFollowing == followRequest.IdUserFollowing).FirstOrDefault();

                    if (follow == null)
                        return null;

                    return follow;
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
