using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NexFit.Backend.Data;
using NexFit.Models;
using NexFit.Backend.Data;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace NexFit.Controllers
{
    public class AuthController : Controller
    {
        private readonly MongoDbContext _context;

        public AuthController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string fullName, string password, string transactionId)
        {
            var existingUser = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                ModelState.AddModelError("", "This email is already registered or request is pending.");
                return View();
            }

            // User apna actual password khud enter karega jo hash ho kar save hoga
            string hashedPassword = BCryptNet.HashPassword(password);

            var pendingMember = new ApplicationUser
            {
                Email = email,
                FullName = fullName,
                PaymentTransactionId = transactionId,
                IsApproved = false,         // Pending Admin fee approval
                MustChangePassword = false, // No need to force change since they set it themselves
                PasswordHash = hashedPassword,
                Roles = new List<string> { "Member" },
                RegistrationDate = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(pendingMember);

            ViewBag.SuccessMessage = "Your registration request and fee reference have been submitted! Please wait for Admin to verify your payment and activate your account.";
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

            if (user == null || !BCryptNet.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or security credentials.");
                return View();
            }

            if (!user.IsApproved)
            {
                ModelState.AddModelError("", "Your membership request is pending fee verification from management.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName)
            };
            foreach (var role in user.Roles) { claims.Add(new Claim(ClaimTypes.Role, role)); }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            if (user.Roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}