using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NexFit.Backend.Data;
using NexFit.Models;
using NexFit.Backend.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace NexFit.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MongoDbContext _context;

        public AdminController(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pendingRequests = await _context.Users.Find(u => u.IsApproved == false).ToListAsync();
            ViewBag.PendingRequests = pendingRequests;

            ViewBag.TotalMembers = await _context.Users.CountDocumentsAsync(u => u.IsApproved == true);
            ViewBag.SystemStatus = "ONLINE";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            // Ab password touch nahi hoga, sirf account activate hoga!
            var update = Builders<ApplicationUser>.Update
                .Set(u => u.IsApproved, true);

            await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string userId)
        {
            await _context.Users.DeleteOneAsync(u => u.Id == userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrainer(string email, string fullName)
        {
            // Trainers ko admin khud banayega toh unka default password generate hoga
            string trainerTempPassword = BCryptNet.HashPassword("Trainer@NexFit");

            var newTrainer = new ApplicationUser
            {
                Email = email,
                FullName = fullName,
                IsApproved = true,
                MustChangePassword = true, // Force trainer to change it on first login
                PasswordHash = trainerTempPassword,
                Roles = new List<string> { "Trainer" }
            };

            await _context.Users.InsertOneAsync(newTrainer);
            return RedirectToAction("Index");
        }
    }
}