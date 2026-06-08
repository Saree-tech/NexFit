using Microsoft.AspNetCore.SignalR;

namespace NexFit.Hubs
{
    public class GymHub : Hub
    {
        // Server restart tak value yaad rahegi
        private static int _lastCapacity = 0;

        public async Task UpdateGymCapacity(int currentCapacity)
        {
            _lastCapacity = currentCapacity;
            await Clients.All.SendAsync("ReceiveCapacityUpdate", currentCapacity);
        }

        // Jab naya member connect ho — usse last value bhejo
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveCapacityUpdate", _lastCapacity);
            await base.OnConnectedAsync();
        }
    }
}