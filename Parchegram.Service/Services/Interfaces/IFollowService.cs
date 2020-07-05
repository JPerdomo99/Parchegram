using Parchegram.Model.Request.Follow;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Service.Services.Interfaces
{
    interface IFollowService
    {
        public bool AddFollow(FollowRequest followRequest);
        public bool RemoveFollow(FollowRequest followRequest); 
    }
}
