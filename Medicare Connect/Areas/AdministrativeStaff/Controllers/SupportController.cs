using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class SupportController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // Mock support tickets for demonstration
        var supportTickets = new List<object>
        {
            new { Id = 1, PatientName = "john.doe@example.com", Subject = "Appointment Rescheduling", Priority = "Medium", Status = "Open", CreatedDate = DateTime.Now.AddHours(-2) },
            new { Id = 2, PatientName = "jane.smith@example.com", Subject = "Payment Issue", Priority = "High", Status = "In Progress", CreatedDate = DateTime.Now.AddHours(-4) },
            new { Id = 3, PatientName = "bob.wilson@example.com", Subject = "Prescription Refill", Priority = "Low", Status = "Resolved", CreatedDate = DateTime.Now.AddHours(-6) },
            new { Id = 4, PatientName = "alice.brown@example.com", Subject = "Insurance Verification", Priority = "Medium", Status = "Open", CreatedDate = DateTime.Now.AddHours(-8) }
        };

        return View(supportTickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(object model)
    {
        // Here you would typically save the support ticket
        TempData["SuccessMessage"] = "Support ticket created successfully.";
        return RedirectToAction(nameof(Index));
    }
} 