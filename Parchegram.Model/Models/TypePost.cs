using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class TypePost
    {
        public TypePost()
        {
            Post = new HashSet<Post>();
        }

        public int Id { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Post> Post { get; set; }
    }
}
