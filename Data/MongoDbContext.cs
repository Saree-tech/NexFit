using MongoDB.Driver;
using NexFit.Models;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using BCryptNet = BCrypt.Net.BCrypt;

namespace NexFit.Backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            // 1. Fetch connection string from appsettings.json
            string connString = configuration.GetConnectionString("MongoDB");

            // 2. Fallback defense line: Agar appsettings se null mile toh crash na ho
            if (string.IsNullOrEmpty(connString))
            {
                connString = "mongodb://localhost:27017";
            }

            var client = new MongoClient(connString);

            // Database target layer match
            _database = client.GetDatabase("NexFitDb");

            // Execute local automated admin injection checks
            SeedAdminUser();
        }

        public IMongoCollection<ApplicationUser> Users =>
            _database.GetCollection<ApplicationUser>("Users");

        private void SeedAdminUser()
        {
            // Check if our unique master admin already exists
            var adminUser = Users.Find(u => u.Email == "admin@nexfit.com").FirstOrDefault();

            if (adminUser == null)
            {
                // Generate a fresh structural safe hash locally matching your assembly build context
                string secureHash = BCryptNet.HashPassword("nexfitadm");

                var masterAdmin = new ApplicationUser
                {
                    Email = "admin@nexfit.com",
                    FullName = "System Administrator",
                    IsApproved = true,
                    MustChangePassword = false,
                    PasswordHash = secureHash,
                    Roles = new List<string> { "Admin" },
                    PaymentTransactionId = "MASTER-BYPASS",
                    IsSubscriptionActive = true
                };

                Users.InsertOne(masterAdmin);
            }
            else if (!adminUser.IsApproved || adminUser.MustChangePassword)
            {
                // Backup update: Ensure configurations are not blocking login handshakes
                var update = Builders<ApplicationUser>.Update
                    .Set(u => u.IsApproved, true)
                    .Set(u => u.MustChangePassword, false)
                    .Set(u => u.PasswordHash, BCryptNet.HashPassword("nexfitadm"));

                Users.UpdateOne(u => u.Email == "admin@nexfit.com", update);
            }
        }
    }
}