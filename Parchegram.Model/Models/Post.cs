using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class Post
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int IdUser { get; set; }
        public int IdTypePost { get; set; }
        public string PathFile { get; set; }
        public DateTime Date { get; set; }

        public virtual TypePost IdTypePostNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
