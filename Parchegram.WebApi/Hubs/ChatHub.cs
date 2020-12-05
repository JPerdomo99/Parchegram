using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parchegram.Model.Models;

namespace Parchegram.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static ICollection<ChatClient> _chatClients = new List<ChatClient>();
        
        public void NewInstance(string nameUser)
        {
            if (ChatClientExists(nameUser))
                ChangeIdConnection(Context.ConnectionId, nameUser); 
            else
                AddClient(Context.ConnectionId, nameUser);
        }

        public async Task SendMessage(string sender, string receiver, string message)
        {
            await Clients.Clients(GetConnectionId(sender), GetConnectionId(receiver)).SendAsync("ReceiveMessage", sender, receiver, message);
            using (var db = new ParchegramDBContext())
            {
                try
                {
                    User senderDB = await db.User.Where(u => u.NameUser.Equals(sender)).FirstOrDefaultAsync();
                    User receiverDB = await db.User.Where(u => u.NameUser.Equals(receiver)).FirstOrDefaultAsync();
                    await db.Message.AddAsync(new Message(senderDB.Id, receiverDB.Id, message));
                    await db.SaveChangesAsync();
                } 
                catch (Exception e)
                {
                    string exceptionMessage = e.Message;
                } 
            }
        }

        private void AddClient(string idConnection, string nameUser)
        {
            _chatClients.Add(new ChatClient(idConnection, nameUser));
        }

        private bool ChatClientExists(string nameUser)
        {
            return _chatClients.Where(c => c.NameUser.Equals(nameUser)).Any();
        }

        private void ChangeIdConnection(string idConnection, string nameUser)
        {
            ChatClient chatClient = _chatClients.Where(c => c.NameUser.Equals(nameUser)).FirstOrDefault();
            if (chatClient.ConnectionId != idConnection)
                chatClient.ConnectionId = idConnection;
        }

        private string GetConnectionId(string clientName)
        {
            return _chatClients.Where(c => c.NameUser.Equals(clientName)).Select(c => c.ConnectionId).FirstOrDefault();
        }

        private ChatClient GetClient(string clientName)
        {
            return _chatClients.Where(cc => cc.NameUser.Equals(clientName)).FirstOrDefault();
        }
    }
}
