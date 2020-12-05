using System;
using System.Collections.Generic;
using System.Text;

namespace Parchegram.Model.Response.Message
{
    public class MessageListResponse
    {
        public int IdMessage { get; set; }

        public string NameUserSender { get; set; }

        public string NameUserReceiver { get; set; }

        public string MessageText { get; set; }

        public DateTime Date { get; set; }
    }
}
