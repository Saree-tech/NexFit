using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexFit.Services;
using System.Security.Claims;

namespace NexFit.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly MongoDbRepository _db;

        public AttendanceController(MongoDbRepository db)
        {
            _db = db;
        }

        // =========================
        // ATTENDANCE PAGE
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _db.GetUserByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // NULL FIX
            user.AttendanceLogs ??= new List<DateTime>();

            return View(user);
        }

        // =========================
        // CHECK IN
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _db.GetUserByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // NULL FIX
            user.AttendanceLogs ??= new List<DateTime>();

            // =========================
            // PREVENT DOUBLE ATTENDANCE
            // =========================
            bool alreadyMarkedToday =
                user.LastAttendanceDate.HasValue &&
                user.LastAttendanceDate.Value.Date ==
                DateTime.UtcNow.Date;

            if (alreadyMarkedToday)
            {
                TempData["Error"] =
                    "Attendance already marked today.";

                return RedirectToAction("Index");
            }

            // STORE OLD DATE FOR STREAK
            var previousAttendanceDate =
                user.LastAttendanceDate;

            // =========================
            // MARK ATTENDANCE
            // =========================
            user.AttendanceLogs.Add(DateTime.UtcNow);

            user.TotalVisits += 1;
            user.TotalVisitsThisMonth += 1;

            user.LastAttendanceDate = DateTime.UtcNow;

            user.IsInsideGym = true;

            user.LastCheckInTime = DateTime.UtcNow;

            // =========================
            // WORKOUT STREAK
            // =========================
            if (previousAttendanceDate.HasValue)
            {
                int difference =
                    (DateTime.UtcNow.Date -
                     previousAttendanceDate.Value.Date).Days;

                if (difference == 1)
                {
                    user.WorkoutStreak += 1;
                }
                else if (difference > 1)
                {
                    user.WorkoutStreak = 1;
                }
            }
            else
            {
                user.WorkoutStreak = 1;
            }

            // =========================
            // SAVE
            // =========================
            await _db.UpdateUserAsync(user);

            TempData["Success"] =
                "Attendance marked successfully.";

            return RedirectToAction("Index");
        }

        // =========================
        // CHECK OUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _db.GetUserByEmailAsync(email);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            user.IsInsideGym = false;

            user.LastCheckOutTime = DateTime.UtcNow;

            await _db.UpdateUserAsync(user);

            TempData["Success"] =
                "Checked out successfully.";

            return RedirectToAction("Index");
        }
    }
}