using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class RecordsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public RecordsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // This would typically show a list of patients with recent records
        // For now, redirect to Patients since that's where record access happens
        return RedirectToAction("Index", "Patients");
    }
} 