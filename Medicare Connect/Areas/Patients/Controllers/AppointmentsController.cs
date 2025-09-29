using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.Patients.Models;
using Stripe.Checkout;
using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using ModelAppointmentStatus = Medicare_Connect.Areas.Patients.Models.AppointmentStatus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class AppointmentsController : Controller
{
	private readonly IEmailSender _emailSender;
	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ITimeslotService _timeslotService;
	private readonly IAppointmentService _appointmentService;

	public AppointmentsController(IEmailSender emailSender, ApplicationDbContext context, UserManager<IdentityUser> userManager, ITimeslotService timeslotService, IAppointmentService appointmentService)
	{
		_emailSender = emailSender;
		_context = context;
		_userManager = userManager;
		_timeslotService = timeslotService;
		_appointmentService = appointmentService;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		// Existing in-memory list for display for now
		var key = AppointmentStore.GetPatientKey(User);
		var list = AppointmentStore.GetList(key);
		if (!list.Any())
		{
			// Seed a few demo rows
			var a1 = new Appointment { PatientKey = key, AppointmentDate = DateTime.Today.AddDays(2).AddHours(10.5), AppointmentType = "General Checkup", DoctorName = "Dr. Thandeka Khumalo", Location = "Room 3", PriceCents = AppointmentStore.GetPriceCentsFor("General Checkup") };
			AppointmentStore.Add(key, a1);
			ReminderStore.AddAppointmentReminders(key, a1.Id, a1.AppointmentType, a1.AppointmentDate);

			var a2 = new Appointment { PatientKey = key, AppointmentDate = DateTime.Today.AddDays(-7).AddHours(9), AppointmentType = "Lab Results", DoctorName = "Dr. Thandeka Khumalo", Status = ModelAppointmentStatus.Completed, IsPaid = true, PriceCents = AppointmentStore.GetPriceCentsFor("Lab Results") };
			AppointmentStore.Add(key, a2);
		}

		// Load doctors for booking UI
		var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
		ViewData["Doctors"] = doctors.Select(d => new { d.Id, Name = d.Email }).ToList();

		// Load DB appointments for this patient
		var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
		var dbAppointments = await _context.Appointments
			.Where(a => a.PatientId == patientId)
			.OrderByDescending(a => a.AppointmentDate)
			.ThenByDescending(a => a.StartTime)
			.ToListAsync();
		ViewData["DbAppointments"] = dbAppointments;

		var view = list
			.OrderByDescending(a => a.AppointmentDate)
			.ToList();
		return View(view);
	}

	[HttpGet]
	public async Task<IActionResult> AvailableTimeslots(string doctorId, DateTime date)
	{
		if (string.IsNullOrWhiteSpace(doctorId)) return BadRequest("doctorId required");
		var slots = await _timeslotService.GetAvailableTimeslotsAsync(doctorId, date);
		var result = slots.Select(s => new { s.Id, start = s.StartTime, end = s.EndTime, label = s.StartTime.ToString("HH:mm") + " - " + s.EndTime.ToString("HH:mm") });
		return Json(result);
	}

	private static decimal GetPriceForType(string type)
	{
		return type switch
		{
			"General Checkup" => 500.00m,
			"Follow-up" => 300.00m,
			"Lab Results" => 200.00m,
			"Vaccination" => 400.00m,
			_ => 350.00m
		};
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> BookSlot(string doctorId, Guid timeslotId, string appointmentType, string? notes)
	{
		if (string.IsNullOrWhiteSpace(doctorId) || timeslotId == Guid.Empty || string.IsNullOrWhiteSpace(appointmentType))
		{
			TempData["ApptError"] = "Please select doctor, date and timeslot.";
			return RedirectToAction(nameof(Index));
		}

		var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
		var patientDisplayName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? "Patient");

		// Lookup timeslot
		var day = DateTime.Today; // used only for price label

		// Build appointment entity based on timeslot
		var timeslotList = await _timeslotService.GetDoctorScheduleAsync(doctorId, DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
		var slot = timeslotList.FirstOrDefault(s => s.Id == timeslotId);
		if (slot == null)
		{
			TempData["ApptError"] = "Selected timeslot is no longer available.";
			return RedirectToAction(nameof(Index));
		}

		var entity = new AppointmentEntity
		{
			PatientId = patientId,
			DoctorId = doctorId,
			TimeslotId = timeslotId,
			AppointmentType = appointmentType,
			AppointmentDate = slot.StartTime.Date,
			StartTime = slot.StartTime.TimeOfDay,
			EndTime = slot.EndTime.TimeOfDay,
			DurationMinutes = slot.DurationMinutes,
			Status = Data.Entities.AppointmentStatus.Booked,
			Location = "Main Clinic",
			Notes = notes,
			Price = GetPriceForType(appointmentType),
			InsuranceAmount = null,
			PatientResponsibility = GetPriceForType(appointmentType),
			PaymentStatus = "Pending"
		};

		var created = await _appointmentService.CreateAppointmentAsync(entity);
		if (!created)
		{
			TempData["ApptError"] = "Unable to book the selected timeslot. It may have just been taken.";
			return RedirectToAction(nameof(Index));
		}

		// Create Stripe checkout similar to existing flow
		var baseUrl = $"{Request.Scheme}://{Request.Host}";
	

        var options = new SessionCreateOptions
		{
			Mode = "payment",
			SuccessUrl = baseUrl + Url.Action("Success", "Payments", new { area = "Patients", appointmentId = entity.Id })!,
			CancelUrl = baseUrl + Url.Action("Cancel", "Payments", new { area = "Patients", appointmentId = entity.Id })!,
			LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					Quantity = 1,
					PriceData = new SessionLineItemPriceDataOptions
					{
						Currency = "zar",
						UnitAmountDecimal = entity.Price * 100,
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = $"Appointment: {appointmentType}",
							Description = $"Doctor {doctorId} on {slot.StartTime:yyyy-MM-dd HH:mm}"
						}
					}
				}
			}
		};
		var service = new SessionService();
		var session = service.Create(options);
		entity.StripeSessionId = session.Id;
		await _context.SaveChangesAsync();

		try
		{
			var payment = new PaymentEntity
			{
				PatientId = patientId,
				PaymentType = PaymentType.Appointment,
				Description = $"Appointment: {appointmentType}",
				Amount = entity.Price,
				Status = PaymentStatus.Pending,
				StripeSessionId = session.Id,
				ReferenceId = entity.Id.ToString(),
				Notes = $"Doctor {doctorId} on {slot.StartTime:yyyy-MM-dd HH:mm}",
				CreatedAt = DateTime.UtcNow
			};
			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();
		}
		catch { }

		return Redirect(session.Url);
	}

	// Existing in-memory booking kept for backward compatibility
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Book(AppointmentEditModel model)
	{
		if (!ModelState.IsValid)
		{
			TempData["ApptError"] = "Please correct the errors and try again.";
			return RedirectToAction(nameof(Index));
		}
		var key = AppointmentStore.GetPatientKey(User);
		var displayName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value
				   ?? (User.Identity?.Name ?? "Patient");
		var appt = new Appointment
		{
			PatientKey = key,
			PatientDisplayName = displayName,
			AppointmentDate = model.AppointmentDate!.Value,
			AppointmentType = model.AppointmentType,
			DoctorName = model.DoctorName,
			Notes = model.Notes,
			Location = "Main Clinic",
			PriceCents = AppointmentStore.GetPriceCentsFor(model.AppointmentType)
		};
		AppointmentStore.Add(key, appt);
		ReminderStore.AddAppointmentReminders(key, appt.Id, appt.AppointmentType, appt.AppointmentDate);

		var toEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
		if (!string.IsNullOrWhiteSpace(toEmail))
		{
			var subject = $"Reminder scheduled: {appt.AppointmentType} on {appt.AppointmentDate:dd MMM yyyy HH:mm}";
			var body = $"<p>Sawubona {displayName},</p><p>We have scheduled reminders for your appointment on <strong>{appt.AppointmentDate:ddd, dd MMM yyyy HH:mm}</strong>.</p><ul><li>24 hours before</li><li>2 hours before</li></ul><p>Doctor: {appt.DoctorName}</p>";
			_ = _emailSender.SendEmailAsync(toEmail, subject, body);
		}

		var unitAmountCents = appt.PriceCents;

		//var baseUrl = $"{Request.Scheme}://{Request.Host}";
		var baseUrl = "https://5c4lhtj6-7205.uks1.devtunnels.ms";

        var options = new SessionCreateOptions
		{
			Mode = "payment",
			SuccessUrl = baseUrl + Url.Action("Success", "Payments", new { area = "Patients", appointmentId = appt.Id })!,
			CancelUrl = baseUrl + Url.Action("Cancel", "Payments", new { area = "Patients", appointmentId = appt.Id })!,
			LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					Quantity = 1,
					PriceData = new SessionLineItemPriceDataOptions
					{
						Currency = "zar",
						UnitAmount = unitAmountCents,
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = $"Appointment: {model.AppointmentType}",
							Description = $"Doctor: {model.DoctorName} on {model.AppointmentDate:yyyy-MM-dd HH:mm}"
						}
					}
				}
			}
		};
		var service = new SessionService();
		var session = service.Create(options);
		appt.StripeSessionId = session.Id;

       
        try
		{
			var payment = new PaymentEntity
			{
				PatientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? key,
				PaymentType = PaymentType.Appointment,
				Description = $"Appointment: {model.AppointmentType}",
				Amount = appt.PriceCents / 100.0m,
				Status = PaymentStatus.Pending,
				StripeSessionId = session.Id,
				ReferenceId = appt.Id.ToString(),
				Notes = $"Doctor: {model.DoctorName} on {model.AppointmentDate:yyyy-MM-dd HH:mm}",
				CreatedAt = DateTime.UtcNow
			};

			
			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();

            
        }
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to save payment record: {ex.Message}");
		}

		return Redirect(session.Url);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Cancel(Guid id)
	{
		var key = AppointmentStore.GetPatientKey(User);
		var appt = AppointmentStore.FindById(key, id);
		if (appt != null)
		{
			appt.Status = ModelAppointmentStatus.Cancelled;
			ReminderStore.RemoveAppointmentReminders(key, appt.Id);
		}
		TempData["ApptSuccess"] = "Appointment cancelled.";
		return RedirectToAction(nameof(Index));
	}
}


