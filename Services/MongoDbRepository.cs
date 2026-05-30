using MongoDB.Driver;
using NexFit.Models;

namespace NexFit.Services
{
    public class MongoDbRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<ApplicationUser> _users;

        public MongoDbRepository(IConfiguration configuration)
        {
            var client = new MongoClient(
                configuration.GetConnectionString("MongoDb"));

            _database = client.GetDatabase("NexFitDb");

            _users = _database.GetCollection<ApplicationUser>("Users");
        }

        // =========================
        // USERS COLLECTION ACCESS
        // =========================

        public IMongoCollection<ApplicationUser> Users => _users;

        // =========================
        // GET USER BY EMAIL
        // =========================

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _users
                .Find(x => x.Email == email)
                .FirstOrDefaultAsync();
        }

        // =========================
        // GET USER BY ID
        // =========================

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
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

        public async Task CreateUserAsync(ApplicationUser user)
        {
            await _users.InsertOneAsync(user);
        }

        // =========================
        // UPDATE USER
        // =========================

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _users.ReplaceOneAsync(x => x.Id == user.Id, user);
        }

        // =========================
        // DELETE USER
        // =========================

        public async Task DeleteUserAsync(string id)
        {
            await _users.DeleteOneAsync(x => x.Id == id);
        }

        public void RecalculateFitness(ApplicationUser user)
        {
            // =========================
            // BMI
            // =========================
            if (user.Height > 0 && user.Weight > 0)
            {
                double heightM = user.Height / 100.0;
                user.BMI = user.Weight / (heightM * heightM);
            }

            // =========================
            // IDEAL / TARGET WEIGHT
            // (simple fitness formula)
            // =========================
            if (user.Height > 0)
            {
                // BMI ideal range approx 22
                double heightM = user.Height / 100.0;
                user.GoalWeight = 22 * (heightM * heightM);
            }

            // =========================
            // PROGRESS
            // =========================
            if (user.GoalWeight > 0)
            {
                double progress = (1 - (user.Weight / user.GoalWeight)) * 100;
                user.GoalCompletion = (int)Math.Clamp(progress, 0, 100);
            }
        }
    }
}