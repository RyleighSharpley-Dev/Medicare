using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Nurses.Models;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class CareController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // This would typically show a dashboard of patient care activities
        // For now, redirect to Patients since that's where care management happens
        return RedirectToAction("Index", "Patients");
    }
} 