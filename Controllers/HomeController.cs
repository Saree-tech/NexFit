using Microsoft.AspNetCore.Mvc;
using NexFit.Services;
using System.Security.Claims;
using MongoDB.Driver;

namespace NexFit.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDbRepository _db;

        public HomeController(MongoDbRepository db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // =========================
            // DEFAULT VALUES
            // =========================

            ViewBag.UserName = "Member";

            ViewBag.TotalVisits = 0;
            ViewBag.WorkoutStreak = 0;
            ViewBag.GoalCompletion = 0;
            ViewBag.CaloriesBurned = 0;

            ViewBag.BMI = 0;
            ViewBag.CurrentWeight = 0;
            ViewBag.TargetWeight = 0;

            ViewBag.WaterIntake = 0;
            ViewBag.SleepHours = 0;

            ViewBag.ChurnRisk = 0;

            ViewBag.SubscriptionActive = false;

            // =========================
            // LIVE GYM CAPACITY
            // =========================

            ViewBag.GymCapacity = 100;
            ViewBag.CurrentGymUsers = 0;

            // =========================
            // COUNT USERS INSIDE GYM
            // =========================

            var activeUsers = await _db.Users
                .Find(x => x.IsInsideGym == true)
                .ToListAsync();

            ViewBag.CurrentGymUsers = activeUsers.Count;

            // =========================
            // AUTH CHECK
            // =========================

            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _db.GetUserByEmailAsync(email);

                    if (user != null)
                    {
                        // =========================
                        // USER INFO
                        // =========================

                        ViewBag.UserName =
                            user.FullName ?? "Member";

                        // =========================
                        // FITNESS STATS
                        // =========================

                        ViewBag.TotalVisits =
                            user.TotalVisits;

                        ViewBag.WorkoutStreak =
                            user.WorkoutStreak;

                        ViewBag.CaloriesBurned =
                            user.CaloriesBurned;

                        // =========================
                        // BODY STATS
                        // =========================

                        ViewBag.BMI =
                            user.BMI;

                        ViewBag.CurrentWeight =
                            user.Weight;

                        ViewBag.TargetWeight =
                            user.GoalWeight;

                        ViewBag.WaterIntake =
                            user.WaterIntake;

                        ViewBag.SleepHours =
                            user.SleepHours;

                        // =========================
                        // AI ANALYTICS
                        // =========================

                        ViewBag.ChurnRisk =
                            user.ChurnRisk;

                        ViewBag.GoalCompletion =
                            user.GoalCompletion;

                        // =========================
                        // MEMBERSHIP CHECK
                        // =========================

                        bool isAdmin =
                            user.Roles != null &&
                            user.Roles.Contains("Admin");

                        if (isAdmin)
                        {
                            ViewBag.SubscriptionActive = true;
                        }
                        else
                        {
                            ViewBag.SubscriptionActive =
                                user.MembershipEndDate.HasValue &&
                                user.MembershipEndDate.Value.Date >= DateTime.UtcNow.Date;
                        }
                    }
                }
            }

            return View();
        }
    }
}