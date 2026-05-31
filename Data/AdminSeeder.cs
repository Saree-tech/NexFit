using MongoDB.Driver;
using NexFit.Models;
using NexFit.Services;

namespace NexFit.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            IServiceProvider services)
        {
            using var scope =
                services.CreateScope();

            var db =
                scope.ServiceProvider
                .GetRequiredService<MongoDbRepository>();

            // =====================================
            // CHECK EXISTING ADMIN
            // =====================================

            var existingAdmin =
                await db.Users
                .Find(x =>
                    x.Email == "admin@nexfit.com")
                .FirstOrDefaultAsync();

            if (existingAdmin != null)
            {
                return;
            }

            // =====================================
            // CREATE ADMIN
            // =====================================

            var admin = new ApplicationUser
            {
                FullName =
                    "System Administrator",

                Email =
                    "admin@nexfit.com",

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        "Admin123!"),

                Roles =
                    new List<string>
                    {
                        "Admin"
                    },

                IsApproved = true,

                MustChangePassword = false,

                SubscriptionActive = true,

                MembershipType = "Premium",

                PaymentTransactionId =
                    "MASTER-BYPASS",

                RegistrationDate =
                    DateTime.UtcNow,

                LastLoginDate =
                    DateTime.UtcNow,

                MembershipStartDate =
                    DateTime.UtcNow,

                MembershipEndDate =
                    DateTime.UtcNow.AddYears(10),

                Age = 25,

                Gender = "Male",

                Height = 175,

                Weight = 75,

                GoalWeight = 70,

                FitnessGoal =
                    "Maintain Fitness",

                ActivityLevel =
                    "Advanced",

                Bio =
                    "NexFit System Administrator",

                ProfileImage =
                    "/images/default-user.png",

                WorkoutStreak = 1,

                TotalVisits = 1,

                TotalVisitsThisMonth = 1,

                CaloriesBurned = 0,

                CompletedWorkouts = 0,

                GoalCompletion = 100,

                ChurnRisk = 0,

                BMI = 24,

                WaterIntake = 3,

                SleepHours = 8,

                LastAttendanceDate =
                    DateTime.UtcNow,

                AttendanceLogs =
                    new List<DateTime>
                    {
                        DateTime.UtcNow
                    },

                NextClass =
                    "No Class Scheduled",

                IsInsideGym = false,

                LastCheckInTime = null,

                LastCheckOutTime = null
            };

            // =====================================
            // INSERT ADMIN
            // =====================================

            await db.Users.InsertOneAsync(admin);

            Console.WriteLine(
                "NexFit admin seeded successfully.");
        }
    }
}
