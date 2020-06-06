using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TestSingalR.SignalR
{
    public class ChatHub : Hub
    {
        public async Task SendStatus(string status)
        {
            await Clients.All.SendAsync("ReceiveStatus", status);
        }

        public async Task AssociateJob(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }
}
