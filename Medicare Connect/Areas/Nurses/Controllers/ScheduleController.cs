using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class ScheduleController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // This would typically show nurse schedules, shifts, and assignments
        // For now, show a placeholder view
        return View();
    }
} 