using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class ScheduleController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;

	public ScheduleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
	{
		_context = context;
		_userManager = userManager;
	}

	private string GetDoctorId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
	}

	[HttpGet]
	public async Task<IActionResult> Index(DateTime? start, DateTime? end)
	{
		var doctorId = GetDoctorId();
		var from = (start ?? DateTime.Today).Date;
		var to = (end ?? DateTime.Today.AddDays(7)).Date.AddDays(1).AddTicks(-1);
		var slots = await _context.DoctorTimeslots
			.Where(s => s.DoctorId == doctorId && s.StartTime >= from && s.StartTime <= to)
			.OrderBy(s => s.StartTime)
			.ToListAsync();
		ViewData["From"] = from.Date;
		ViewData["To"] = to.Date;
		return View(slots);
	}

	[HttpGet]
	public async Task<IActionResult> Edit(Guid id)
	{
		var doctorId = GetDoctorId();
		var slot = await _context.DoctorTimeslots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctorId);
		if (slot == null) return RedirectToAction(nameof(Index));
		return View(slot);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(Guid id, DateTime date, TimeSpan startTime, int durationMinutes, bool isAvailable)
	{
		var doctorId = GetDoctorId();
		var slot = await _context.DoctorTimeslots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctorId);
		if (slot == null)
		{
			TempData["MsgError"] = "Timeslot not found.";
			return RedirectToAction(nameof(Index));
		}

		var newStart = date.Date + startTime;
		var newEnd = newStart.AddMinutes(durationMinutes);

		// Prevent changes if slot has an active appointment
		var hasActiveAppt = await _context.Appointments.AnyAsync(a => a.TimeslotId == id && a.Status != AppointmentStatus.Cancelled);
		if (hasActiveAppt && (slot.StartTime != newStart || slot.EndTime != newEnd))
		{
			TempData["MsgError"] = "Cannot reschedule a slot that has active appointments.";
			return RedirectToAction(nameof(Index));
		}

		// Check overlap against other slots
		var overlap = await _context.DoctorTimeslots.AnyAsync(s => s.DoctorId == doctorId && s.Id != id && s.StartTime < newEnd && s.EndTime > newStart);
		if (overlap)
		{
			TempData["MsgError"] = "Updated time overlaps with another slot.";
			return RedirectToAction(nameof(Index));
		}

		slot.StartTime = newStart;
		slot.EndTime = newEnd;
		slot.DurationMinutes = durationMinutes;
		slot.IsAvailable = isAvailable;
		slot.UpdatedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync();
		TempData["Msg"] = "Timeslot updated.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleAvailability(Guid id)
	{
		var doctorId = GetDoctorId();
		var slot = await _context.DoctorTimeslots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctorId);
		if (slot == null)
		{
			TempData["MsgError"] = "Timeslot not found.";
			return RedirectToAction(nameof(Index));
		}
		slot.IsAvailable = !slot.IsAvailable;
		slot.UpdatedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync();
		TempData["Msg"] = slot.IsAvailable ? "Timeslot unblocked." : "Timeslot blocked.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> BlockRange(DateTime start, DateTime end)
	{
		var doctorId = GetDoctorId();
		var from = start.Date;
		var to = end.Date.AddDays(1);
		var slots = await _context.DoctorTimeslots
			.Where(s => s.DoctorId == doctorId && s.StartTime >= from && s.StartTime < to)
			.ToListAsync();
		foreach (var s in slots)
		{
			s.IsAvailable = false;
			s.UpdatedAt = DateTime.UtcNow;
		}
		await _context.SaveChangesAsync();
		TempData["Msg"] = "Selected range blocked.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(DateTime date, TimeSpan startTime, int durationMinutes)
	{
		if (durationMinutes < 15 || durationMinutes > 480)
		{
			TempData["MsgError"] = "Duration must be between 15 and 480 minutes.";
			return RedirectToAction(nameof(Index));
		}
		var doctorId = GetDoctorId();
		var startDt = date.Date + startTime;
		var endDt = startDt.AddMinutes(durationMinutes);

		// Prevent overlaps with existing slots
		var overlap = await _context.DoctorTimeslots.AnyAsync(s => s.DoctorId == doctorId && s.StartTime < endDt && s.EndTime > startDt);
		if (overlap)
		{
			TempData["MsgError"] = "Timeslot overlaps with existing slot.";
			return RedirectToAction(nameof(Index));
		}

		_context.DoctorTimeslots.Add(new DoctorTimeslot
		{
			DoctorId = doctorId,
			StartTime = startDt,
			EndTime = endDt,
			DurationMinutes = durationMinutes,
			IsAvailable = true,
			IsRecurring = false
		});
		await _context.SaveChangesAsync();
		TempData["Msg"] = "Timeslot created.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id)
	{
		var doctorId = GetDoctorId();
		var slot = await _context.DoctorTimeslots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctorId);
		if (slot == null)
		{
			TempData["MsgError"] = "Timeslot not found.";
			return RedirectToAction(nameof(Index));
		}
		var hasActiveAppt = await _context.Appointments.AnyAsync(a => a.TimeslotId == id && a.Status != AppointmentStatus.Cancelled);
		if (hasActiveAppt)
		{
			TempData["MsgError"] = "Cannot delete a slot with active appointments.";
			return RedirectToAction(nameof(Index));
		}
		_context.DoctorTimeslots.Remove(slot);
		await _context.SaveChangesAsync();
		TempData["Msg"] = "Timeslot deleted.";
		return RedirectToAction(nameof(Index));
	}
} 