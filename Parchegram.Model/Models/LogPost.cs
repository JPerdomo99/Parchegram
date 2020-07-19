using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class LogPost
    {
        public int IdUser { get; set; }
        public int IdPost { get; set; }
        public DateTime Date { get; set; }

        public virtual Post IdPostNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
