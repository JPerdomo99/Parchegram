using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Follow
{
    public class FollowRequest
    {
        [Required]
        public int IdUserFollower { get; set; }

        [Required]
        public int IdUserFollowing { get; set; }
    }
}
