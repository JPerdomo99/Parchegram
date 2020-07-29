using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class UserImageProfile
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string PathImageS { get; set; }
        public string PathImageM { get; set; }

        public virtual User IdUserNavigation { get; set; }
    }
}
