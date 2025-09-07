using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.Pharmacists.Controllers;

[Area("Pharmacists")]
[Authorize(Roles = "Pharmacists")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}


