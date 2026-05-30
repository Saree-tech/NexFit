using MongoDB.Driver;

namespace NexFit.Services
{
    public class DashboardService
    {
        private readonly MongoDbRepository _db;
        public DashboardService(MongoDbRepository context)
        {
            _db = context;
        }

        // Total Approved Members
        public async Task<int> GetTotalApprovedMembers()
        {
            return (int)await _db.Users
                .CountDocumentsAsync(u => u.IsApproved);
        }

        // Members who visited gym this month
        public async Task<int> GetTodayActiveMembers()
        {
            return (int)await _db.Users
                .CountDocumentsAsync(u => u.TotalVisitsThisMonth > 0);
        }

        // Active subscriptions
        public async Task<int> GetActiveSubscriptions()
        {
            return (int)await _db.Users
                .CountDocumentsAsync(u => u.SubscriptionActive);
        }

        // Dynamic gym capacity %
        public async Task<string> GetGymCapacity()
        {
            int totalMembers = await GetTotalApprovedMembers();

            if (totalMembers == 0)
                return "0%";

            int activeMembers = await GetTodayActiveMembers();

            double percentage =
                ((double)activeMembers / totalMembers) * 100;

            return $"{Math.Round(percentage)}%";
        }
    }

}
