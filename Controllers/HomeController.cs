using Microsoft.AspNetCore.Identity;
using SUPFLY.Areas.Identity.Data; // Ensure this is your correct Identity User class
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SUPFLY.Models;

namespace SUPFLY.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        // 🔑 TEMPORARY METHOD TO GENERATE HASH
        public IActionResult GetHash()
        {
            var user = new SUPFLYUser(); // Create a dummy user object (needed for the TUser generic parameter)
            var hasher = new PasswordHasher<SUPFLYUser>();

            // Generate the hash for the password "Admin123!"
            string hash = hasher.HashPassword(user, "Admin123!");

            // Display the hash on the page
            ViewData["Hash"] = hash;

            // IMPORTANT: Copy the hash from the displayed text on the screen!
            return View();
        }
        // 🔑 END TEMPORARY METHOD

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
