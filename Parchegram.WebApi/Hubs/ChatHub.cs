using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Parchegram.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static ICollection<ChatClient> _chatClients = new List<ChatClient>();
        private static ICollection<ChatGroup> _chatGroups = new List<ChatGroup>();
        
        public void NewInstance(string nameUser)
        {
            if (ChatClientExists(nameUser))
                ChangeIdConnection(Context.ConnectionId, nameUser); 
            else
                AddClient(Context.ConnectionId, nameUser);
        }

        private async Task AddToGroup(string nameUserSender, string nameUserReceiver) 
        {
            // Not exists group between Sender and Receiver
            if (_chatGroups.Where(cg => cg.GroupName.Equals(nameUserSender.Concat(nameUserReceiver))).Any() == false)
            {
                ChatClient chatClientSender = _chatClients.Where(cc => cc.NameUser.Equals(nameUserSender)).FirstOrDefault();
                ChatClient chatClientReceiver = null;
                ChatGroup chatGroup = null;
                string chatGroupName = nameUserSender.Concat(nameUserReceiver).ToString();

                // In case UserReceiver is not in chatClients
                if (_chatClients.Where(cc => cc.NameUser.Equals(nameUserReceiver)).Any() == false)
                {
                    chatClientReceiver = new ChatClient(string.Empty, nameUserReceiver);
                    chatGroup = new ChatGroup(chatClientSender, chatClientReceiver);
                    _chatGroups.Add(chatGroup);

                    await Groups.AddToGroupAsync(chatClientSender.ConnectionId, chatGroupName);
                } else
                {
                    chatClientReceiver = _chatClients.Where(cc => cc.NameUser.Equals(nameUserReceiver)).FirstOrDefault();
                    chatGroup = new ChatGroup(chatClientSender, chatClientReceiver);
                    _chatGroups.Add(chatGroup);

                    await Groups.AddToGroupAsync(chatClientSender.ConnectionId, chatGroupName);
                    await Groups.AddToGroupAsync(chatClientReceiver.ConnectionId, chatGroupName);
                }
            }
        }

        private string GetGroupName(string nameUserSender, string nameUserReceiver)
        {
            return _chatGroups.Where(cg => cg.GroupName.Equals(nameUserSender.Concat(nameUserReceiver))).Select(cg => cg.GroupName).FirstOrDefault();
        }

        public async Task SendMessage(string nameUserSender, string nameUserReceiver, string message)
        {
            //await Clients.Client(GetConnectionIdReceiver(nameUserReceiver)).SendAsync("ReceiveMessage", nameUserSender, message);
            await Clients.Group(_chatGroups.Where(cg => cg.GroupName.Equals(nameUserSender.Concat(nameUserReceiver)))
                .Select(cg => cg.GroupName).FirstOrDefault()).SendAsync("ReceiveMessage", message);
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

        private string GetConnectionIdReceiver(string nameUserReceiver)
        {
            return _chatClients.Where(c => c.NameUser.Equals(nameUserReceiver)).Select(c => c.ConnectionId).FirstOrDefault();
        }
    }
}
