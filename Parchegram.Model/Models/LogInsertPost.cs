using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class LogInsertPost
    {
        public int? Id { get; set; }
        public string Description { get; set; }
        public int? IdUser { get; set; }
        public int? IdTypePost { get; set; }
        public DateTime? Date { get; set; }
    }
}
