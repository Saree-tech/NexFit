using MongoDB.Driver;
using NexFit.Models;

namespace NexFit.Services
{
    public class MongoDbRepository
    {
        private readonly IMongoDatabase _database;

        // =========================
        // COLLECTIONS
        // =========================

        private readonly IMongoCollection<ApplicationUser> _users;
        private readonly IMongoCollection<DailyVitals> _dailyVitals;

        // =========================
        // CONSTRUCTOR
        // =========================

        public MongoDbRepository(IConfiguration configuration)
        {
            var client = new MongoClient(
                configuration.GetConnectionString("MongoDb"));

            _database = client.GetDatabase("NexFitDb");

            _users =
                _database.GetCollection<ApplicationUser>("Users");

            _dailyVitals =
                _database.GetCollection<DailyVitals>("DailyVitals");
        }

        // =========================
        // USERS COLLECTION ACCESS
        // =========================

        public IMongoCollection<ApplicationUser> Users => _users;

        // =========================
        // DAILY VITALS COLLECTION
        // =========================

        public IMongoCollection<DailyVitals> DailyVitals
            => _dailyVitals;

        // =========================
        // GET USER BY EMAIL
        // =========================

        public async Task<ApplicationUser?> GetUserByEmailAsync(
            string email)
        {
            return await _users
                .Find(x => x.Email == email)
                .FirstOrDefaultAsync();
        }

        // =========================
        // GET USER BY ID
        // =========================

        public async Task<ApplicationUser?> GetUserByIdAsync(
            string id)
        {
            return await _users
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        // =========================
        // GET ALL USERS
        // =========================

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _users
                .Find(_ => true)
                .ToListAsync();
        }

        // =========================
        // CREATE USER
        // =========================

        public async Task CreateUserAsync(
            ApplicationUser user)
        {
            RecalculateFitness(user);

            await _users.InsertOneAsync(user);
        }

        // =========================
        // UPDATE USER
        // =========================

        public async Task UpdateUserAsync(
            ApplicationUser user)
        {
            RecalculateFitness(user);

            await _users.ReplaceOneAsync(
                x => x.Id == user.Id,
                user);
        }

        // =========================
        // DELETE USER
        // =========================

        public async Task DeleteUserAsync(string id)
        {
            await _users.DeleteOneAsync(x => x.Id == id);
        }

        // =========================
        // SAVE DAILY VITALS
        // =========================

        public async Task SaveDailyVitalsAsync(
            DailyVitals vitals)
        {
            await _dailyVitals.InsertOneAsync(vitals);
        }

        // =========================
        // GET USER DAILY VITALS
        // =========================

        public async Task<List<DailyVitals>>
            GetUserVitalsAsync(string userId)
        {
            return await _dailyVitals
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.Date)
                .ToListAsync();
        }

        // =========================
        // GET LATEST DAILY VITALS
        // =========================

        public async Task<DailyVitals?>
            GetLatestVitalsAsync(string userId)
        {
            return await _dailyVitals
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.Date)
                .FirstOrDefaultAsync();
        }

        // =========================
        // FITNESS CALCULATIONS
        // =========================

        public void RecalculateFitness(
            ApplicationUser user)
        {
            // =========================
            // BMI
            // =========================

            if (user.Height > 0 &&
                user.Weight > 0)
            {
                double heightM =
                    user.Height / 100.0;

                user.BMI =
                    user.Weight /
                    (heightM * heightM);
            }

            // =========================
            // TARGET WEIGHT
            // =========================

            if (user.Height > 0)
            {
                double heightM =
                    user.Height / 100.0;

                user.GoalWeight =
                    22 * (heightM * heightM);
            }

            // =========================
            // GOAL COMPLETION
            // =========================

            if (user.GoalWeight > 0)
            {
                double difference =
                    Math.Abs(
                        user.GoalWeight -
                        user.Weight);

                double progress =
                    100 - ((difference /
                    user.GoalWeight) * 100);

                user.GoalCompletion =
                    (int)Math.Clamp(
                        progress,
                        0,
                        100);
            }
        }
    }
}
