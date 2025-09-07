using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Nurses.Models;
using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Nurses.Controllers;

[Area("Nurses")]
[Authorize(Roles = "Nurses")]
public class PatientsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public PatientsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        
        var patientList = patients.Select(u => new PatientListItem
        {
            PatientKey = u.Id,
            Name = u.Email ?? u.UserName ?? u.Id,
            Email = u.Email ?? u.UserName ?? string.Empty,
            LastVisit = GetLastVisitDate(u.Id),
            Status = "Active"
        }).OrderBy(p => p.Name).ToList();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            patientList = patientList.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        ViewBag.SearchTerm = searchTerm;
        return View(patientList);
    }

    [HttpGet]
    public async Task<IActionResult> Care(string patientKey)
    {
        var patient = await _userManager.FindByIdAsync(patientKey);
        if (patient == null)
        {
            return NotFound();
        }

        var model = new PatientCareViewModel
        {
            PatientKey = patientKey,
            PatientName = patient.Email ?? patient.UserName ?? patientKey,
            PatientEmail = patient.Email ?? patient.UserName ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Care(PatientCareViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Here you would typically save the care information to a database
        // For now, we'll just show a success message
        TempData["CareSaved"] = true;
        return RedirectToAction(nameof(Care), new { patientKey = model.PatientKey });
    }

    private DateTime? GetLastVisitDate(string patientKey)
    {
        // Get the most recent appointment date for the patient
        var allAppointments = AppointmentStore.GetAllByPatientKey();
        if (allAppointments.TryGetValue(patientKey, out var appointments))
        {
            return appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .OrderByDescending(a => a.AppointmentDate)
                .FirstOrDefault()?.AppointmentDate;
        }
        return null;
    }
} 