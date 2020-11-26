using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.WebApi
{
    public class ChatGroup
    {
        private ChatClient _chatClientSender;

        private ChatClient _chatClientReceiver;

        public ChatGroup(ChatClient chatClientSender, 
            ChatClient chatClientReceiver)
        {
            _chatClientSender = chatClientSender;
            _chatClientReceiver = chatClientReceiver;
        }

        public ChatClient ChatClientSender
        {
            get { return _chatClientSender; }
            set { _chatClientSender = value; }
        }

        public ChatClient ChatClientReceiver
        {
            get { return _chatClientReceiver; }
            set { _chatClientReceiver = value; }
        }

        public string GroupName
        {
            get { return _chatClientSender.NameUser + _chatClientReceiver.NameUser; }
        }
    }
}
