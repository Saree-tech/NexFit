using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexFit.Models;
using NexFit.Services;
using System.Security.Claims;

namespace NexFit.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly MongoDbRepository _db;

        public ProfileController(MongoDbRepository db)
        {
            _db = db;
        }

        // =========================
        // GET PROFILE
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login", "Auth");

            var user = await _db.GetUserByEmailAsync(email);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View(user);
        }

        // =========================
        // UPDATE PROFILE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ApplicationUser updatedUser)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Login", "Auth");

            var user = await _db.GetUserByEmailAsync(email);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // =========================
            // PROFILE UPDATE
            // =========================
            user.FullName = updatedUser.FullName;
            user.Age = updatedUser.Age;
            user.Gender = updatedUser.Gender;

            user.Height = updatedUser.Height;
            user.Weight = updatedUser.Weight;

            user.FitnessGoal = updatedUser.FitnessGoal;
            user.ActivityLevel = updatedUser.ActivityLevel;
            user.Bio = updatedUser.Bio;

            // ❌ REMOVE THIS LINE (VERY IMPORTANT)
            // user.GoalWeight = updatedUser.GoalWeight;

            // =========================
            // BMI CALCULATION (SAFE)
            // =========================
            if (user.Height > 0 && user.Weight > 0)
            {
                double heightM = user.Height / 100.0;

                user.BMI = Math.Round(
                    user.Weight / (heightM * heightM),
                    2
                );
            }
            else
            {
                user.BMI = 0;
            }

            // =========================
            // AUTO GOAL WEIGHT (FIX)
            // =========================
            if (user.Height > 0)
            {
                double heightM = user.Height / 100.0;

                // Ideal BMI = 22 (fitness standard)
                user.GoalWeight = Math.Round(
                    22 * (heightM * heightM),
                    2
                );
            }

            // =========================
            // GOAL PROGRESS (FIXED)
            // =========================
            if (user.GoalWeight > 0 && user.Weight > 0)
            {
                double progress =
                    (1 - (user.Weight / user.GoalWeight)) * 100;

                user.GoalCompletion = (int)Math.Clamp(progress, 0, 100);
            }
            else
            {
                user.GoalCompletion = 0;
            }

            // =========================
            // SAVE
            // =========================
            await _db.UpdateUserAsync(user);

            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction("Index");
        }
    }
}