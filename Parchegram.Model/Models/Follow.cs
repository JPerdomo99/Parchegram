using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class Follow
    {
        public int IdUserFollower { get; set; }
        public int IdUserFollowing { get; set; }

        public virtual User IdUserFollowerNavigation { get; set; }
        public virtual User IdUserFollowingNavigation { get; set; }
    }
}
