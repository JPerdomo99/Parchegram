using Parchegram.Model.Response.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IFollowService
    {
        public Task<Response> Add(string nameUserFollower, int idUserFollowing);
        public Task<Response> Delete(string nameUserFollower, int idUserFollowing); 
    }
}
