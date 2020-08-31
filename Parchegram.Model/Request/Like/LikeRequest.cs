using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parchegram.Model.Request.Like
{
    public class LikeRequest
    {
        [Required]
        public int IdPost { get; set; }

        [Required]
        public string NameUser { get; set; }
    }
}
