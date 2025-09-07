using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class TriageController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // This would typically show a triage dashboard with patient priority levels
        // For now, show a placeholder view
        return View();
    }
} 