using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}


