using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.WebApi
{
    public class ChatClient
    {
        private string _connectionId;
        private string _nameUser;

        public ChatClient(string idConnection, string nameUser)
        {
            _connectionId = idConnection;
            _nameUser = nameUser;
        }

        public string ConnectionId
        {
            get { return _connectionId; }
            set { _connectionId = value; }
        }

        public string NameUser
        {
            get { return _nameUser; }
        }
    }
}
