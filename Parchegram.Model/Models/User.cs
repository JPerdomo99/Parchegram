using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class User
    {
        public User()
        {
            FollowIdUserFollowerNavigation = new HashSet<Follow>();
            FollowIdUserFollowingNavigation = new HashSet<Follow>();
            LogPost = new HashSet<LogPost>();
            Post = new HashSet<Post>();
        }

        public int Id { get; set; }
        public string NameUser { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateBirth { get; set; }
        public bool ConfirmEmail { get; set; }
        public string CodeConfirmEmail { get; set; }

        public virtual ICollection<Follow> FollowIdUserFollowerNavigation { get; set; }
        public virtual ICollection<Follow> FollowIdUserFollowingNavigation { get; set; }
        public virtual ICollection<LogPost> LogPost { get; set; }
        public virtual ICollection<Post> Post { get; set; }
    }
}
