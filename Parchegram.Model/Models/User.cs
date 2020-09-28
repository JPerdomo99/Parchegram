using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class User
    {
        public User()
        {
            Comment = new HashSet<Comment>();
            FollowIdUserFollowerNavigation = new HashSet<Follow>();
            FollowIdUserFollowingNavigation = new HashSet<Follow>();
            Like = new HashSet<Like>();
            Post = new HashSet<Post>();
            UserImageProfile = new HashSet<UserImageProfile>();
        }

        public int Id { get; set; }
        public string NameUser { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateBirth { get; set; }
        public bool ConfirmEmail { get; set; }
        public string CodeConfirmEmail { get; set; }

        public virtual ICollection<Comment> Comment { get; set; }
        public virtual ICollection<Follow> FollowIdUserFollowerNavigation { get; set; }
        public virtual ICollection<Follow> FollowIdUserFollowingNavigation { get; set; }
        public virtual ICollection<Like> Like { get; set; }
        public virtual ICollection<Post> Post { get; set; }
        public virtual ICollection<UserImageProfile> UserImageProfile { get; set; }
    }
}
