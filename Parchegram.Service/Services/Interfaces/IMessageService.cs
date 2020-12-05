using Parchegram.Model.Response.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Interfaces
{
    public interface IMessageService
    {
        public Task<Response> GetMessages(string sender, string receiver);
    }
}
