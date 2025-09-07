using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.Managers.Controllers;

[Area("Managers")]
[Authorize(Roles = "Managers/Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}


