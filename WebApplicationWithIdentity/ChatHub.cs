using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationWithIdentity.DAL;
using WebApplicationWithIdentity.Models;

namespace WebApplicationWithIdentity
{
    public class ChatHub : Hub
    {
        private readonly ChatContext _chatContext;

         public ChatHub(ChatContext chatContext)
        {
            _chatContext = chatContext;

        }
        public async Task SendMessage(string user, string message)
        {
            var userId = Context.UserIdentifier;
            var chatMessage = new ChatMessage
            {
                Sender = user,
                Message = message,
                TimeStamp = DateTime.Now,
                IdSender = userId
            };
            try
            {
                await _chatContext.ChatMessages.AddAsync(chatMessage);
                await _chatContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
