using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // This would typically show and allow editing of nurse profile information
        // For now, show a placeholder view
        return View();
    }
} 