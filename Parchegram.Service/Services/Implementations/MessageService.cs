using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parchegram.Model.Models;
using Parchegram.Model.Response.General;
using Parchegram.Model.Response.Message;
using Parchegram.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parchegram.Service.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }

        public async Task<Response> GetMessages(string sender, string receiver)
        {
            Response response = new Response();
            try
            {
                using (var db = new ParchegramDBContext())
                {
                    User senderDB = await db.User.Where(u => u.NameUser.Equals(sender)).FirstOrDefaultAsync();
                    if (senderDB == null)
                        return response.GetResponse("El sender no existe", 0, null);
                    User receiverDB = await db.User.Where(u => u.NameUser.Equals(receiver)).FirstOrDefaultAsync();
                    if (receiverDB == null)
                        return response.GetResponse("El receiver no existe", 0, null);
                    ICollection<MessageListResponse> messages = await (from message in db.Message
                                                           join senderM in db.User on message.IdUserSender equals senderM.Id
                                                           join receiverM in db.User on message.IdUserReceiver equals receiverM.Id
                                                           where (message.IdUserSender.Equals(senderDB.Id) && message.IdUserReceiver.Equals(receiverDB.Id))
                                                            || (message.IdUserSender.Equals(receiverDB.Id) && message.IdUserReceiver.Equals(senderDB.Id))
                                                           orderby message.Date
                                                           select new MessageListResponse
                                                           {
                                                               IdMessage = message.Id,
                                                               NameUserSender = senderM.NameUser,
                                                               NameUserReceiver = receiverM.NameUser,
                                                               MessageText = message.MessageText,
                                                               Date = message.Date
                                                           }).ToListAsync();
                    return response.GetResponse("Mensajes obtenidos correctamente", 1, messages);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return response.GetResponse($"Ha ocurrido un error inesperado: {e.Message}", 0, null);
            }
        }
    }
}
