// ================================
// ProgressController.cs
// ================================

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NexFit.Models;
using NexFit.Models.ViewModels;
using NexFit.Services;
using System.Security.Claims;

namespace NexFit.Controllers
{
    public class ProgressController : Controller
    {
        private readonly MongoDbRepository _db;

    public ProgressController(
        MongoDbRepository db)
        {
            _db = db;
        }

        // =====================================
        // INDEX PAGE
        // =====================================

        public async Task<IActionResult> Index()
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            var user = await _db.Users
                .Find(x => x.Id == userId)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                bool subscriptionActive =
                    user.SubscriptionActive;

                if (user.MembershipEndDate.HasValue)
                {
                    subscriptionActive =
                        user.MembershipEndDate.Value >
                        DateTime.UtcNow;
                }

                user.SubscriptionActive =
                    subscriptionActive;
            }

            var vitals = await _db.DailyVitals
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.Date)
                .ToListAsync();

            var vm =
                new ProgressViewModel
                {
                    User = user,
                    VitalsHistory = vitals
                };

            return View(vm);
        }

        // =====================================
        // SAVE VITALS
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVitals(
            DailyVitals model)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            var user = await _db.Users
                .Find(x => x.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            model.UserId = userId;

            model.Date = DateTime.UtcNow;

            if (model.Height <= 0)
            {
                model.Height = user.Height;
            }

            // =====================================
            // BMI
            // =====================================

            double bmi = 0;

            if (model.Height > 0 &&
                model.Weight > 0)
            {
                double heightM =
                    model.Height / 100.0;

                bmi =
                    model.Weight /
                    (heightM * heightM);
            }

            bmi = Math.Round(bmi, 1);

            // =====================================
            // GOAL COMPLETION
            // =====================================

            int goalCompletion = 0;

            // WATER

            if (model.WaterIntake >= 3)
            {
                goalCompletion += 20;
            }

            // SLEEP

            if (model.SleepHours >= 7)
            {
                goalCompletion += 20;
            }

            // WORKOUT

            if (model.WorkoutMinutes >= 45)
            {
                goalCompletion += 20;
            }

            // STEPS

            if (model.StepsWalked >= 8000)
            {
                goalCompletion += 20;
            }

            // WEIGHT GOAL

            if (user.GoalWeight > 0)
            {
                double difference =
                    Math.Abs(model.Weight - user.GoalWeight);

                if (difference <= 1)
                {
                    goalCompletion += 20;
                }
                else if (difference <= 3)
                {
                    goalCompletion += 15;
                }
                else if (difference <= 5)
                {
                    goalCompletion += 10;
                }
                else if (difference <= 10)
                {
                    goalCompletion += 5;
                }
            }

            goalCompletion =
                Math.Min(goalCompletion, 100);

            model.FitnessScore =
                goalCompletion;

            // =====================================
            // SAVE HISTORY
            // =====================================

            await _db.DailyVitals
                .InsertOneAsync(model);

            // =====================================
            // CHURN RISK
            // =====================================

            int churnRisk = 0;

            if (model.WorkoutMinutes < 20)
            {
                churnRisk += 30;
            }

            if (model.SleepHours < 6)
            {
                churnRisk += 20;
            }

            if (model.WaterIntake < 2)
            {
                churnRisk += 20;
            }

            if (model.StepsWalked < 4000)
            {
                churnRisk += 20;
            }

            // =====================================
            // WORKOUT STREAK
            // =====================================

            int streak =
                user.WorkoutStreak;

            if (model.WorkoutMinutes > 0)
            {
                if (user.LastAttendanceDate.HasValue &&
                    user.LastAttendanceDate.Value.Date ==
                    DateTime.UtcNow.Date.AddDays(-1))
                {
                    streak += 1;
                }
                else
                {
                    streak = 1;
                }
            }

            // =====================================
            // TOTAL CALORIES
            // =====================================

            int totalCalories =
                user.CaloriesBurned +
                model.CaloriesBurned;

            // =====================================
            // MEMBERSHIP
            // =====================================

            bool subscriptionActive =
                user.SubscriptionActive;

            if (user.MembershipEndDate.HasValue)
            {
                subscriptionActive =
                    user.MembershipEndDate.Value >
                    DateTime.UtcNow;
            }

            // =====================================
            // UPDATE USER
            // =====================================

            var update =
                Builders<ApplicationUser>
                .Update

                .Set(x => x.Weight, model.Weight)
                .Set(x => x.Height, model.Height)
                .Set(x => x.BMI, bmi)

                .Set(x => x.WaterIntake, model.WaterIntake)
                .Set(x => x.SleepHours, model.SleepHours)

                .Set(x => x.CaloriesBurned, totalCalories)
                .Set(x => x.GoalCompletion, goalCompletion)
                .Set(x => x.ChurnRisk, churnRisk)
                .Set(x => x.WorkoutStreak, streak)

                .Set(x => x.SubscriptionActive, subscriptionActive)

                .Set(x => x.LastAttendanceDate, DateTime.UtcNow);

            await _db.Users.UpdateOneAsync(
                x => x.Id == userId,
                update);

            return RedirectToAction("Index");
        }
    }

}
