using Microsoft.AspNetCore.Mvc;

namespace NexFit.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Publicly visible gym statistics (Laraib's SignalR integration can update this later)
            ViewBag.LiveGymCapacity = "45%";
            ViewBag.ActiveMembersToday = 87;
            ViewBag.GymStatus = "OPEN";

            // Promotion Banners & Offers Data
            ViewBag.PromoTitle = "Summer Fitness Revolution 2026!";
            ViewBag.PromoDiscount = "Get 20% OFF on all Yearly Memberships. Valid till next week!";

            return View();
        }
    }
}