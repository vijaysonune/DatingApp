using DatingApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.SignalR
{
    [Authorize]
    public class PresenceHub : Hub 
    {
        private readonly PresenceTracker presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            this.presenceTracker = presenceTracker;
        }
        
        public override async Task OnConnectedAsync()
        {

            var isOnline=  await presenceTracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);

            if(isOnline)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUserName());

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline= await presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);

            if(isOffline)
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());


            await base.OnDisconnectedAsync(exception);
        }
    }
}
