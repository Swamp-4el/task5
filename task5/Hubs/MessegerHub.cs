using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using task5.Contexts;
using task5.Managers;
using task5.Models;
using task5.Models.DbModels;

namespace task5.Hubs
{
    public class MessegerHub : Hub
    {
        private readonly MessengerContext _dbContext;
        private readonly IUserManager _userManager;

        public MessegerHub(IUserManager userManager, MessengerContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _userManager.Start(_dbContext);
        }

        public async Task Send(MessageViewModel model)
        {
            if (!_userManager.IsUserCreated(_dbContext, model.RecipientId))
            {
                await Clients.Caller.SendAsync("InvalidRecipientName");
            }

            if (_userManager.IsUserCreated(_dbContext, model.SenderId) &&
                _userManager.IsUserCreated(_dbContext, model.RecipientId))
            {
                await SendNewMessage(model);
            }
        }

        private async Task SendNewMessage(MessageViewModel model)
        {
            var message = GetMessageFromModel(model);

            _dbContext.Messages.Add(message);
            _dbContext.SaveChanges();

            await Clients.Clients(_userManager.GetConnectionsUser(message.RecipientId))
                .SendAsync("NewMessage", message);
            

            if (message.SenderId != message.RecipientId) 
            {
                await Clients.Clients(_userManager.GetConnectionsUser(message.SenderId))
                .SendAsync("NewMessage", message);
            }
        }

        public async Task ConnectUser(string userName)
        {
            if (!_userManager.IsUserCreated(_dbContext, userName))
            {
                await AddUser(userName);
                await Clients.All.SendAsync("AddUser", userName);
            }
            _userManager.AddConnection(_dbContext, userName, Context.ConnectionId);

            await GetMessages(userName);
        }

        public async Task DisconnectUser()
        {
            _userManager.RemoveConnection(Context.ConnectionId);
            await Clients.Caller.SendAsync("Disconnect");
        }

        public async Task GetMessagesBetweenUsers(string firstUser, string secondUser)
        {
            if (string.IsNullOrWhiteSpace(firstUser) ||
                string.IsNullOrWhiteSpace(secondUser))
                return;

            var messages = await _dbContext.Messages
                .Where(m => m.RecipientId == firstUser && m.SenderId == secondUser ||
                       m.RecipientId == secondUser && m.SenderId == firstUser)
                .Select(m => new
                {
                    Time = m.Time.ToString("hh:mm dd:MM:yyyy"),
                    RecipientId = m.RecipientId,
                    SenderId = m.SenderId,
                    Data = m.Data,
                    Title = m.Title,
                })
                .ToListAsync();

            await Clients.Clients(_userManager.GetConnectionsUser(firstUser))
                .SendAsync("GetMessages", messages);
        }

        public async Task GetMessages(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return;

            var messages = await _dbContext.Messages
                .Where(m => m.RecipientId == userName || m.SenderId == userName)
                .Select(m => new
                {
                    Time = m.Time.ToString("hh:mm dd:MM:yyyy"),
                    RecipientId = m.RecipientId,
                    SenderId = m.SenderId,
                    Data = m.Data,
                    Title = m.Title,
                })
                .ToListAsync();

            await Clients.Clients(_userManager.GetConnectionsUser(userName))
                .SendAsync("GetMessages", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _userManager.RemoveConnection(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task AddUser(string userName)
        {
            _dbContext.Users.Add(new User
            {
                Id = userName,
                Create = DateTime.Now,
            });
            await _dbContext.SaveChangesAsync();
        }

        private Message GetMessageFromModel(MessageViewModel model)
        {
            return new Message
            {
                Data = model.Data,
                Time = DateTime.Now,
                RecipientId = model.RecipientId,
                SenderId = model.SenderId,
                Title = model.Title,
            };
        }
    }
}
