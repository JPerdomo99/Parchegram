using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Send message to group with GroupName = client1Name + client2Name or client2Name + client1Name
        public async Task SendMessage(string client1Name, string client2Name, string message)
        {
            ChatGroup chatGroup = GetChatGroup(client1Name, client2Name);
            await Clients.Group(chatGroup.GroupName).SendAsync("ReceiveMessage", client1Name, message);
        }

        public async Task AddToGroup(string client1Name, string client2Name)
        {
            // Find chatGroup with groupName client1Name + client2Name or client2Name + client1Name
            ChatGroup chatGroup = GetChatGroup(client1Name, client2Name);
            if (chatGroup == null)
            {
                chatGroup = new ChatGroup();
                string groupName = CreateGroupName(client1Name, client2Name);

                // Agg client1 to group
                // Se supone que el cliente ya existe
                if (ChatClientExists(client1Name))
                {
                    await Groups.AddToGroupAsync(GetConnectionId(client1Name), groupName);
                    chatGroup.Client1 = GetClient(client1Name);
                }
                                        
                // Agg client2 to group
                // Se supone que este es el recibidor
                if (ChatClientExists(client2Name))
                {
                    await Groups.AddToGroupAsync(GetConnectionId(client2Name), groupName);
                    chatGroup.Client2 = GetClient(client2Name);
                } else
                {
                    chatGroup.Client2 = new ChatClient(string.Empty, client2Name);
                    _chatClients.Add(chatGroup.Client2);
                }

                // Add chatGroup
                _chatGroups.Add(chatGroup);
            }
        }

        private ChatGroup GetChatGroup(string client1Name, string client2Name)
        {
            return _chatGroups.Where(cg => cg.GroupName.Equals(string.Concat(client1Name, client2Name)) 
                    || cg.GroupName.Equals(string.Concat(client2Name, client1Name))).FirstOrDefault();
        }

        private string CreateGroupName(string clientName, string clientName2)
        {
            return string.Concat(clientName, clientName2);
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
