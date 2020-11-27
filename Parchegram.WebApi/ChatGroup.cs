using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.WebApi
{
    public class ChatGroup
    {
        private ChatClient _client1;

        private ChatClient _client2;

        public ChatGroup()
        {
        }

        public ChatGroup(ChatClient client1, 
            ChatClient client2)
        {
            _client1 = client1;
            _client2 = client2;
        }

        public ChatClient Client1
        {
            get { return _client1; }
            set { _client1 = value; }
        }

        public ChatClient Client2
        {
            get { return _client2; }
            set { _client2 = value; }
        }

        public string GroupName
        {
            get { return _client1.NameUser + _client2.NameUser; }
        }
    }
}
