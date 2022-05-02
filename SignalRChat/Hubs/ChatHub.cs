using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> connIds = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public async Task SendMessage(string from, string to, string message)
        {
            var context = this.Context;
            
            var client = this.Clients;

            if (!connIds.ContainsKey(from))
            {
                connIds.Add(from, context.ConnectionId);
            }

            if (connIds.ContainsKey(to))
            {
                await client.Clients(connIds[to]).SendAsync("ReceiveMessage", from, message);
            }
        }
    }
}
