using Microsoft.AspNetCore.SignalR;

namespace NexFit.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string senderName, string message)
        {
            // Dono users ko message bhejo with sender name aur time
            await Clients.All.SendAsync("ReceiveMessage", senderName, message, DateTime.Now.ToString("hh:mm tt"));
        }
    }
}