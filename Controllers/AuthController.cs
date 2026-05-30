using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NexFit.Models;
using NexFit.Services;
using System.Security.Claims;

namespace NexFit.Controllers
{
    public class AuthController : Controller
    {
        private readonly MongoDbRepository _db;

        public AuthController(MongoDbRepository db)
        {
            _db = db;
        }

        // =========================
        // REGISTER
        // =========================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string email,
            string fullName,
            string password,
            string transactionId)
        {
            var existingUser = await _db.Users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            var newUser = new ApplicationUser
            {
                Email = email,
                FullName = fullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),

                PaymentTransactionId = transactionId,
                IsApproved = false,
                MustChangePassword = false,

                MembershipType = "Basic",
                SubscriptionActive = false,

                FitnessGoal = "Weight Loss",

                Height = 170,
                Weight = 70,
                GoalWeight = 0,

                BMI = 0,
                WaterIntake = 0,
                SleepHours = 0,

                CaloriesBurned = 0,
                CompletedWorkouts = 0,
                ChurnRisk = 0,

                ProfileImage = "/images/default-user.png",
                Bio = "NexFit Member",

                Roles = new List<string> { "Member" },
                AttendanceLogs = new List<DateTime>(),

                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,

                MembershipStartDate = DateTime.UtcNow,
                MembershipEndDate = DateTime.UtcNow.AddMonths(1)
            };

            await _db.Users.InsertOneAsync(newUser);

            ViewBag.SuccessMessage = "Registration successful.";
            return View();
        }

        // =========================
        // LOGIN (FINAL FIXED)
        // =========================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users
                .Find(x => x.Email == email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!user.IsApproved)
            {
                ModelState.AddModelError("", "Account not approved yet.");
                return View();
            }

            // =========================
            // ADMIN BYPASS (IMPORTANT FIX)
            // =========================
            bool isAdmin = user.Roles != null &&
                            user.Roles.Contains("Admin");

            // =========================
            // MEMBERSHIP CHECK (ONLY FOR MEMBERS)
            // =========================
            if (!isAdmin)
            {
                if (!user.MembershipEndDate.HasValue ||
                    user.MembershipEndDate.Value.Date < DateTime.UtcNow.Date)
                {
                    user.SubscriptionActive = false;
                    await _db.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

                    ModelState.AddModelError("", "Membership expired.");
                    return View();
                }
            }

            // =========================
            // UPDATE LOGIN ONLY (NO FAKE DATA)
            // =========================
            user.LastLoginDate = DateTime.UtcNow;
            await _db.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

            // =========================
            // CLAIMS (FIXED)
            // =========================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName ?? "")
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // =========================
            // REDIRECT FIX
            // =========================
            if (isAdmin)
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}