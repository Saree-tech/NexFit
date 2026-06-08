using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using NexFit.Hubs;
using NexFit.Models;
using NexFit.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace NexFit.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MongoDbRepository _db;
        private readonly IHubContext<GymHub> _gymHub;

        public AdminController(MongoDbRepository context, IHubContext<GymHub> gymHub)
        {
            _db = context;
            _gymHub = gymHub;
        }

        public async Task<IActionResult> Index()
        {
            var pendingRequests = await _db.Users.Find(u => u.IsApproved == false).ToListAsync();
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.TotalMembers = await _db.Users.CountDocumentsAsync(u => u.IsApproved == true);
            ViewBag.SystemStatus = "ONLINE";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var update = Builders<ApplicationUser>.Update
                .Set(u => u.IsApproved, true);
            await _db.Users.UpdateOneAsync(u => u.Id == userId, update);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string userId)
        {
            await _db.Users.DeleteOneAsync(u => u.Id == userId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrainer(string email, string fullName)
        {
            string trainerTempPassword = BCryptNet.HashPassword("Trainer@NexFit");
            var newTrainer = new ApplicationUser
            {
                Email = email,
                FullName = fullName,
                IsApproved = true,
                MustChangePassword = true,
                PasswordHash = trainerTempPassword,
                Roles = new List<string> { "Trainer" }
            };
            await _db.Users.InsertOneAsync(newTrainer);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateCapacity(int capacity)
        {
            await _gymHub.Clients.All.SendAsync("ReceiveCapacityUpdate", capacity);
            return Json(new { success = true, capacity = capacity });
        }
    }
}