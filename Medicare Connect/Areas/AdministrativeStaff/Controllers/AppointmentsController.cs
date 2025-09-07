using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.AdministrativeStaff.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class AppointmentsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public AppointmentsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? status, DateTime? date)
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");

        // Get all appointments from the store
        var allAppointments = AppointmentStore.GetAllByPatientKey();
        var appointmentList = new List<AppointmentListItem>();

        foreach (var patientAppointments in allAppointments)
        {
            var patient = patients.FirstOrDefault(p => p.Id == patientAppointments.Key);
            if (patient != null)
            {
                foreach (var appointment in patientAppointments.Value)
                {
                    appointmentList.Add(new AppointmentListItem
                    {
                        Id = appointment.Id.ToString(),
                        PatientName = patient.Email ?? patient.UserName ?? patient.Id,
                        DoctorName = appointment.DoctorName,
                        AppointmentType = appointment.AppointmentType,
                        AppointmentDate = appointment.AppointmentDate,
                        AppointmentTime = appointment.AppointmentDate.TimeOfDay,
                        Status = appointment.Status.ToString(),
                        Location = appointment.Location ?? "Main Clinic",
                        IsConfirmed = appointment.Status == Areas.Patients.Models.AppointmentStatus.Booked,
                        Price = appointment.PriceCents / 100.0m
                    });
                }
            }
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            appointmentList = appointmentList.Where(a => 
                a.PatientName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.DoctorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.AppointmentType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            appointmentList = appointmentList.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (date.HasValue)
        {
            appointmentList = appointmentList.Where(a => a.AppointmentDate.Date == date.Value.Date).ToList();
        }

        appointmentList = appointmentList.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Status = status;
        ViewBag.Date = date;
        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
        ViewBag.Doctors = doctors.Select(d => new { Id = d.Id, Name = d.Email ?? d.UserName ?? d.Id }).ToList();

        return View(appointmentList);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");

        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
        ViewBag.Doctors = doctors.Select(d => new { Id = d.Id, Name = d.Email ?? d.UserName ?? d.Id }).ToList();

        return View(new AppointmentManagementViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentManagementViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctors");

            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            ViewBag.Doctors = doctors.Select(d => new { Id = d.Id, Name = d.Email ?? d.UserName ?? d.Id }).ToList();

            return View(model);
        }

        try
        {
            var appointment = new Areas.Patients.Models.Appointment
            {
                Id = Guid.NewGuid(),
                PatientKey = model.PatientId,
                PatientDisplayName = "", // Will be populated by the store
                AppointmentType = model.AppointmentType,
                AppointmentDate = model.AppointmentDate.Date.Add(model.AppointmentTime),
                DoctorName = "", // Will be populated by the store
                Location = model.Location ?? "Main Clinic",
                Notes = model.Notes,
                Status = Areas.Patients.Models.AppointmentStatus.Booked,
                PriceCents = GetPriceForAppointmentType(model.AppointmentType)
            };

            AppointmentStore.Add(model.PatientId, appointment);

            TempData["SuccessMessage"] = "Appointment created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctors");

            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            ViewBag.Doctors = doctors.Select(d => new { Id = d.Id, Name = d.Email ?? d.UserName ?? d.Id }).ToList();

            return View(model);
        }
    }

    private long GetPriceForAppointmentType(string appointmentType)
    {
        return AppointmentStore.GetPriceCentsFor(appointmentType);
    }
} 