using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NexFit.Hubs
{
    public class GymHub : Hub
    {
        // Jab admin capacity change karega, yeh method call hoga
        public async Task UpdateGymCapacity(int currentCapacity)
        {
            // Yeh sab members ko real-time update bhej dega
            await Clients.All.SendAsync("ReceiveCapacityUpdate", currentCapacity);
        }
    }
}