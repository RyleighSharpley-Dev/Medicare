using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;
using System.Security.Claims;
using Medicare_Connect.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class DashboardController : Controller
{
	private readonly ApplicationDbContext _db;
	public DashboardController(ApplicationDbContext db)
	{
		_db = db;
	}

	public IActionResult Index(string? culture)
	{
		if (!string.IsNullOrWhiteSpace(culture))
		{
			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
				new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
			);
		}

		var key = AppointmentStore.GetPatientKey(User);

		var appts = AppointmentStore.GetList(key)
			.Where(a => a.Status == AppointmentStatus.Booked && a.AppointmentDate >= DateTime.UtcNow)
			.OrderBy(a => a.AppointmentDate)
			.ToList();
		var upcomingCount = appts.Count;
		var nextAppt = appts.FirstOrDefault()?.AppointmentDate;

		var prescriptions = PrescriptionStore.GetPrescriptions(key);
		var activePrescriptions = prescriptions.Where(p => p.Status == PrescriptionStatus.Active).ToList();
		var activeCount = activePrescriptions.Count;
		var totalAvailableRefills = activePrescriptions.Sum(p => Math.Max(0, p.RefillsTotal - p.RefillsUsed));

		var paidAppts = AppointmentStore.GetList(key)
			.Where(a => a.IsPaid)
			.OrderByDescending(a => a.AppointmentDate)
			.ToList();
		long? lastPaymentCents = paidAppts.FirstOrDefault()?.PriceCents;
		DateTime? lastPaymentAt = paidAppts.FirstOrDefault()?.AppointmentDate;

		var reminders = ReminderStore.GetList(key)
			.OrderBy(r => r.DueAt)
			.ToList();
		var remindersCount = reminders.Count(r => !r.IsCompleted);
		var nextReminder = reminders.FirstOrDefault(r => !r.IsCompleted)?.DueAt;

		// DB-backed recent payments for this patient
		var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
		var recentPayments = _db.Payments
			.Where(p => p.PatientId == patientId)
			.OrderByDescending(p => p.CreatedAt)
			.Take(5)
			.ToList();

		var vm = new DashboardViewModel
		{
			UpcomingAppointmentsCount = upcomingCount,
			NextAppointmentUtc = nextAppt,
			ActivePrescriptionsCount = activeCount,
			TotalAvailableRefills = totalAvailableRefills,
			LastPaymentCents = lastPaymentCents,
			LastPaymentAtUtc = lastPaymentAt,
			RemindersCount = remindersCount,
			NextReminderUtc = nextReminder,
			RecentRecords = new(),
			RecentPayments = recentPayments
		};

		return View(vm);
	}
}


