// ================================
// HomeController.cs
// ================================

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NexFit.Services;
using System.Security.Claims;

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
            bool isLoggedIn =
                User.Identity?.IsAuthenticated ?? false;

            // =====================================
            // DEFAULT VALUES
            // =====================================

            ViewBag.UserName = "Member";

            ViewBag.TotalVisits = 0;

            ViewBag.WorkoutStreak = 0;

            ViewBag.GoalCompletion = 0;

            ViewBag.CaloriesBurned = 0;

            ViewBag.BMI = 0.0;

            ViewBag.WaterIntake = 0.0;

            ViewBag.SleepHours = 0.0;

            ViewBag.CurrentWeight = 0.0;

            ViewBag.TargetWeight = 0.0;

            ViewBag.ChurnRisk = 0;

            ViewBag.SubscriptionActive = false;

            ViewBag.GymCapacity = 100;

            ViewBag.CurrentGymUsers = 0;

            // =====================================
            // USER DASHBOARD
            // =====================================

            if (isLoggedIn)
            {
                var userId =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return View();
                }

                var user =
                    await _db.Users
                    .Find(x => x.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return View();
                }

                // =====================================
                // USER INFO
                // =====================================

                ViewBag.UserName =
                    user.FullName;

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

                ViewBag.SubscriptionActive =
                    subscriptionActive;

                // =====================================
                // PROFILE VALUES
                // =====================================

                ViewBag.TargetWeight =
                    Math.Round(user.GoalWeight, 1);

                ViewBag.CurrentWeight =
                    Math.Round(user.Weight, 1);

                ViewBag.WorkoutStreak =
                    user.WorkoutStreak;

                ViewBag.ChurnRisk =
                    user.ChurnRisk;

                ViewBag.CaloriesBurned =
                    user.CaloriesBurned;

                ViewBag.WaterIntake =
                    user.WaterIntake;

                ViewBag.SleepHours =
                    user.SleepHours;

                // =====================================
                // BMI
                // =====================================

                double bmi = 0;

                if (user.Height > 0 &&
                    user.Weight > 0)
                {
                    double heightM =
                        user.Height / 100.0;

                    bmi =
                        user.Weight /
                        (heightM * heightM);
                }

                ViewBag.BMI =
                    Math.Round(bmi, 1);

                // =====================================
                // MONTHLY ATTENDANCE
                // =====================================

                int currentMonth =
                    DateTime.UtcNow.Month;

                int currentYear =
                    DateTime.UtcNow.Year;

                int monthlyVisits =
                    user.AttendanceLogs?
                    .Count(x =>
                        x.Month == currentMonth &&
                        x.Year == currentYear)
                    ?? 0;

                ViewBag.TotalVisits =
                    monthlyVisits;

                // =====================================
                // GET LATEST VITALS
                // =====================================

                var latestVitals =
                    await _db.DailyVitals
                    .Find(x => x.UserId == userId)
                    .SortByDescending(x => x.Date)
                    .FirstOrDefaultAsync();

                if (latestVitals != null)
                {
                    ViewBag.CurrentWeight =
                        Math.Round(
                            latestVitals.Weight, 1);

                    ViewBag.WaterIntake =
                        latestVitals.WaterIntake;

                    ViewBag.SleepHours =
                        latestVitals.SleepHours;

                    ViewBag.CaloriesBurned =
                        latestVitals.CaloriesBurned;

                    // =================================
                    // BMI
                    // =================================

                    if (latestVitals.Height > 0 &&
                        latestVitals.Weight > 0)
                    {
                        double heightM =
                            latestVitals.Height / 100.0;

                        bmi =
                            latestVitals.Weight /
                            (heightM * heightM);

                        ViewBag.BMI =
                            Math.Round(bmi, 1);
                    }

                    // =================================
                    // GOAL COMPLETION
                    // =================================

                    int goalCompletion = 0;

                    // WATER

                    if (latestVitals.WaterIntake >= 3)
                    {
                        goalCompletion += 20;
                    }

                    // SLEEP

                    if (latestVitals.SleepHours >= 7)
                    {
                        goalCompletion += 20;
                    }

                    // WORKOUT

                    if (latestVitals.WorkoutMinutes >= 45)
                    {
                        goalCompletion += 20;
                    }

                    // STEPS

                    if (latestVitals.StepsWalked >= 8000)
                    {
                        goalCompletion += 20;
                    }

                    // WEIGHT GOAL

                    if (user.GoalWeight > 0)
                    {
                        double difference =
                            Math.Abs(
                                latestVitals.Weight -
                                user.GoalWeight);

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

                    ViewBag.GoalCompletion =
                        goalCompletion;

                    // =================================
                    // WORKOUT STREAK
                    // =================================

                    if (latestVitals.WorkoutMinutes > 0)
                    {
                        ViewBag.WorkoutStreak =
                            Math.Max(
                                user.WorkoutStreak,
                                1);
                    }

                    // =================================
                    // CHURN RISK
                    // =================================

                    int churnRisk = 0;

                    if (latestVitals.WorkoutMinutes < 20)
                    {
                        churnRisk += 30;
                    }

                    if (latestVitals.SleepHours < 6)
                    {
                        churnRisk += 20;
                    }

                    if (latestVitals.WaterIntake < 2)
                    {
                        churnRisk += 20;
                    }

                    if (latestVitals.StepsWalked < 4000)
                    {
                        churnRisk += 20;
                    }

                    ViewBag.ChurnRisk =
                        churnRisk;
                }

                // =====================================
                // LIVE GYM CAPACITY
                // =====================================

                int gymCapacity = 100;

                long activeUsers =
                    await _db.Users
                    .CountDocumentsAsync(
                        x => x.IsInsideGym);

                ViewBag.GymCapacity =
                    gymCapacity;

                ViewBag.CurrentGymUsers =
                    (int)activeUsers;
            }

            return View();
        }
    }

}
