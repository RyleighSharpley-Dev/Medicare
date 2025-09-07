using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Data;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class AppointmentsController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ITimeslotService _timeslotService;

	public AppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ITimeslotService timeslotService)
	{
		_context = context;
		_userManager = userManager;
		_timeslotService = timeslotService;
	}

	private string GetDoctorId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
	}

	[HttpGet]
	public async Task<IActionResult> Index(DateTime? date)
	{
		var doctorId = GetDoctorId();
		var startOfDay = (date ?? DateTime.Today).Date;
		var endOfDay = startOfDay.AddDays(1);

		var appts = await _context.Appointments
			.Where(a => a.DoctorId == doctorId && a.AppointmentDate >= startOfDay && a.AppointmentDate < endOfDay)
			.Include(a => a.Patient)
			.Include(a => a.Timeslot)
			.OrderBy(a => a.AppointmentDate)
			.ThenBy(a => a.StartTime)
			.ToListAsync();

		ViewData["SelectedDate"] = startOfDay;
		return View(appts);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Complete(Guid id)
	{
		var doctorId = GetDoctorId();
		var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);
		if (appt != null)
		{
			appt.Status = Medicare_Connect.Data.Entities.AppointmentStatus.Completed;
			appt.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
		}
		TempData["Msg"] = "Marked as completed.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Cancel(Guid id)
	{
		var doctorId = GetDoctorId();
		var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);
		if (appt != null)
		{
			appt.Status = Medicare_Connect.Data.Entities.AppointmentStatus.Cancelled;
			appt.CancelledAt = DateTime.UtcNow;
			appt.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			await _timeslotService.ReleaseTimeslotAsync(appt.TimeslotId);
		}
		TempData["Msg"] = "Appointment cancelled.";
		return RedirectToAction(nameof(Index));
	}
}


