using Microsoft.AspNetCore.Mvc;

namespace NexFit.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Trainer()
        {
            return View("Index");
        }

        public IActionResult Member()
        {
            return View("Index");
        }
    }
}