using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // Mock admin staff profile data
        var profileData = new
        {
            FullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? User.Identity?.Name ?? "Admin Staff",
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "admin@medicare-connect.com",
            PhoneNumber = "+27 12 345 6789",
            Department = "Administration",
            Position = "Administrative Staff",
            EmployeeId = "ADM-001",
            HireDate = "January 1, 2023",
            Supervisor = "Department Manager",
            OfficeLocation = "Main Building, Floor 1",
            WorkSchedule = "Monday - Friday, 8:00 AM - 5:00 PM"
        };

        return View(profileData);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        // Mock admin staff profile data for editing
        var profileData = new
        {
            FullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? User.Identity?.Name ?? "Admin Staff",
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "admin@medicare-connect.com",
            PhoneNumber = "+27 12 345 6789",
            Department = "Administration",
            Position = "Administrative Staff",
            EmployeeId = "ADM-001",
            HireDate = "January 1, 2023",
            Supervisor = "Department Manager",
            OfficeLocation = "Main Building, Floor 1",
            WorkSchedule = "Monday - Friday, 8:00 AM - 5:00 PM"
        };

        return View(profileData);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(object model)
    {
        // Here you would typically update the profile information
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }
} 