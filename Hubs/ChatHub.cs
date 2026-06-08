using Microsoft.AspNetCore.SignalR;

namespace NexFit.Hubs
{
    public class ChatHub : Hub
    {
        // Jab page khule, user apne room mein join kare
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        // Message sirf us room ke logon ko jayega
        public async Task SendMessage(string roomId, string senderName, string message)
        {
            await Clients.Group(roomId).SendAsync(
                "ReceiveMessage",
                senderName,
                message,
                DateTime.Now.ToString("hh:mm tt")
            );
        }
    }
}