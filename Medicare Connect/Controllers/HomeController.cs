using System.Diagnostics;
using Medicare_Connect.Models;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Controllers
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
            // If user is authenticated, redirect to role-specific dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("RedirectToRoleDashboard", "RoleRedirect");
            }
            
            return View();
        }

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
