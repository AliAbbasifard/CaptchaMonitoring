using Entities;
using Microsoft.AspNetCore.SignalR;
using Monitor.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitor.Hubs
{
    public class CaptchaHub : Hub
    {
        private readonly IRoomRepository _roomRepository;
        public CaptchaHub(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }


        /// <summary>
        /// Get a Captcha, set ConnectionId and Expiration for it, and
        /// Call 'GetCaptcha' on other clients, which is our monitor
        /// </summary>
        /// <param name="captcha"></param>
        /// <returns></returns>
        public async Task SendCaptcha(Captcha captcha)
        {
            captcha.ConnectionId = Context.ConnectionId;
            captcha.Expiration = DateTime.Now.AddSeconds(60);

            // this is a js method in Index page 
            await Clients.Others.SendAsync("GetCaptcha", captcha);
        }


        public async Task ValidateCaptcha(string value, string connectionId, DateTime expiration)
        {
            if (DateTime.Now > expiration)
                await this.SendCaptchaExpired(connectionId);
            else
            {
                // get room for this connctionId
                var roomName = await _roomRepository.GetRoom(connectionId);

                // pass value entered by user to Client in this group by Calling 'GetCaptchaResult'
                await Clients.Group(roomName).SendAsync("GetCaptchaResult", value);
            }
        }

        /// <summary>
        /// Called when a capcha expire.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task SendCaptchaExpired(string connectionId)
        {
            // get room for this connctionId
            var roomName = await _roomRepository.GetRoom(connectionId);

            // and call 'GetCaptchaExpired' method for clients in this group 
            await Clients.Group(roomName).SendAsync("GetCaptchaExpired");
        }


        /// <summary>
        /// Called when a client connect to hub
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var roomName = await _roomRepository.CreateRoom(connectionId);

            // create a group for this client with connectionId and roomName
            await Groups.AddToGroupAsync(connectionId, roomName);
        }
    }
}
