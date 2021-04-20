using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.SignalR
{
    public class MessageHub : Hub
    {
       
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker tracker;

        public MessageHub( IMapper mapper, IUnitOfWork unitOfWork,
             IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {            
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;            
            this.presenceHub = presenceHub;
            this.tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(httpContext.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

           var group= await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await unitOfWork.messageRepository
                .GetMessageThread(httpContext.User.GetUserName(), otherUser);

            if (unitOfWork.HasChanges()) await unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessage)
        {
            var username = Context.User.GetUserName();

            if (username == createMessage.RecepientUsername.ToLower())
                throw new HubException("You can not send messages to yourself");

            var sender = await unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var recepient = await unitOfWork.userRepository.GetUserByUsernameAsync(createMessage.RecepientUsername);

            if (recepient == null) throw new HubException("Not Found User");

            var message = new Message
            {
                Sender = sender,
                Recepient = recepient,
                SenderUsername = sender.UserName,
                RecepientUsername = recepient.UserName,
                Content = createMessage.Content
            };

            var groupName = GetGroupName(sender.UserName, recepient.UserName);

            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recepient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connectionIds = await tracker.GetConnectionsForUsers(recepient.UserName);
                if(connectionIds != null)
                {
                    await presenceHub.Clients.Clients(connectionIds).SendAsync("NewMessageReceived", 
                        new { userName = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            unitOfWork.messageRepository.AddMessage(message);

            if (await unitOfWork.Complete())
            {               
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }

            //throw new HubException("Failed to send message");
        }

        private async Task<Group> AddToGroup( string groupName)
        {
            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if (group == null)
            {               
                    group = new Group(groupName);
                    unitOfWork.messageRepository.AddGroup(group);              

            }
           
            group.Connections.Add(connection);
            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
           
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.messageRepository.RemoveConnection(connection);
            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to delete group");
        }


        private string GetGroupName(string caller,string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;

            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }


    }
}
