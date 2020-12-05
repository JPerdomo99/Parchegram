using System;
using System.Collections.Generic;

namespace Parchegram.Model.Models
{
    public partial class Message
    {
        public int Id { get; set; }
        public int IdUserSender { get; set; }
        public int IdUserReceiver { get; set; }
        public string MessageText { get; set; }
        public DateTime Date { get; set; }

        public virtual User IdUserReceiverNavigation { get; set; }
        public virtual User IdUserSenderNavigation { get; set; }
    }

    public partial class Message
    {
        public Message(int idUserSender, int idUserReceiver, string messageText)
        {
            IdUserSender = idUserSender;
            IdUserReceiver = idUserReceiver;
            MessageText = messageText;
            Date = DateTime.Now;
        }
    }
}
