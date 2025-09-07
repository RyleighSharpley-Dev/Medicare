using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using Stripe;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class PaymentsController : Controller
{
	private readonly ApplicationDbContext _context;

	public PaymentsController(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<IActionResult> Index()
	{
		var key = AppointmentStore.GetPatientKey(User);
		var list = AppointmentStore.GetList(key)
			.OrderByDescending(a => a.AppointmentDate)
			.ToList();

		// Also load DB-backed appointments and payment records for this patient
		var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
		var dbAppointments = await _context.Appointments
			.Where(a => a.PatientId == patientId)
			.OrderByDescending(a => a.AppointmentDate)
			.ThenByDescending(a => a.StartTime)
			.ToListAsync();
		ViewData["DbAppointments"] = dbAppointments;

		var payments = await _context.Payments
			.Where(p => p.PatientId == patientId)
			.OrderByDescending(p => p.CreatedAt)
			.ToListAsync();
		ViewData["PaymentHistory"] = payments;

		return View(list);
	}

	[HttpGet]
	public async Task<IActionResult> Success(Guid appointmentId)
	{
		var key = AppointmentStore.GetPatientKey(User);
		var appt = AppointmentStore.FindById(key, appointmentId);
		if (appt != null)
		{
			appt.IsPaid = true;
			
			// Create payment record in database
			try
			{
				var payment = new PaymentEntity
				{
					PatientId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? key,
					PaymentType = PaymentType.Appointment,
					Description = $"Appointment: {appt.AppointmentType}",
					Amount = appt.PriceCents / 100.0m,
					Status = PaymentStatus.Completed,
					StripeSessionId = appt.StripeSessionId,
					ReferenceId = appt.Id.ToString(),
					Notes = $"Doctor: {appt.DoctorName} on {appt.AppointmentDate:yyyy-MM-dd HH:mm}",
					CreatedAt = DateTime.UtcNow,
					CompletedAt = DateTime.UtcNow
				};

				_context.Payments.Add(payment);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				// Log error but don't fail the payment success flow
				// In production, you'd want proper logging here
				System.Diagnostics.Debug.WriteLine($"Failed to save payment record: {ex.Message}");
			}
		}

		// Also update DB-backed appointment if it exists
		try
		{
			var sessionId = appt != null ? appt.StripeSessionId : null;
			var dbAppt = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId || (sessionId != null && a.StripeSessionId == sessionId));
			if (dbAppt != null)
			{
				dbAppt.PaymentStatus = "Completed";
				dbAppt.Status = Data.Entities.AppointmentStatus.Confirmed;
				dbAppt.ConfirmedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}
		catch { }

		TempData["ApptSuccess"] = $"Payment successful. Appointment confirmed (ID: {appointmentId}).";
		return RedirectToAction("Index", "Appointments", new { area = "Patients" });
	}

	[HttpGet]
	public IActionResult Cancel(Guid appointmentId)
	{
		TempData["ApptError"] = $"Payment cancelled. Appointment pending (ID: {appointmentId}).";
		return RedirectToAction("Index", "Appointments", new { area = "Patients" });
	}

	[HttpGet]
	public async Task<IActionResult> RefillPaid(Guid refillRequestId)
	{
		var key = PrescriptionStore.GetPatientKey(User);
		var req = PrescriptionStore.GetRefillRequestsForPatient(key).FirstOrDefault(r => r.Id == refillRequestId);
		if (req != null)
		{
			req.IsPaid = true;
			
			// Create payment record in database
			try
			{
				var payment = new PaymentEntity
				{
					PatientId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? key,
					PaymentType = PaymentType.PrescriptionRefill,
					Description = $"Prescription Refill - {req.PrescriptionId}",
					Amount = req.PriceCents / 100.0m,
					Status = PaymentStatus.Completed,
					ReferenceId = req.Id.ToString(),
					Notes = "Prescription refill payment",
					CreatedAt = DateTime.UtcNow,
					CompletedAt = DateTime.UtcNow
				};

				_context.Payments.Add(payment);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				// Log error but don't fail the payment success flow
				System.Diagnostics.Debug.WriteLine($"Failed to save payment record: {ex.Message}");
			}
			
			TempData["RxSuccess"] = "Refill payment successful.";
		}
		return RedirectToAction("Index", "Prescriptions", new { area = "Patients" });
	}

	[HttpPost]
	[AllowAnonymous] // Stripe webhook needs to be accessible without authentication
	public async Task<IActionResult> StripeWebhook()
	{
		var json = await new StreamReader(Request.Body).ReadToEndAsync();
		
		try
		{
			var stripeEvent = Stripe.EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "your_webhook_secret");
			
			if (stripeEvent.Type == Events.CheckoutSessionCompleted)
			{
				var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
				if (session != null)
				{
					// Find the payment record and update its status
					var payment = await _context.Payments
						.FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
					
					if (payment != null)
					{
						payment.Status = PaymentStatus.Completed;
						payment.CompletedAt = DateTime.UtcNow;
						payment.StripePaymentIntentId = session.PaymentIntentId;
						await _context.SaveChangesAsync();
					}

					// Also update DB appointment by Stripe session
					var dbAppt = await _context.Appointments.FirstOrDefaultAsync(a => a.StripeSessionId == session.Id);
					if (dbAppt != null)
					{
						dbAppt.PaymentStatus = "Completed";
						dbAppt.Status = Data.Entities.AppointmentStatus.Confirmed;
						dbAppt.ConfirmedAt = DateTime.UtcNow;
						await _context.SaveChangesAsync();
					}
				}
			}
			else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
			{
				var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
				if (paymentIntent != null)
				{
					// Find the payment record and update its status
					var payment = await _context.Payments
						.FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);
					
					if (payment != null)
					{
						payment.Status = PaymentStatus.Failed;
						payment.UpdatedAt = DateTime.UtcNow;
						await _context.SaveChangesAsync();
					}
				}
			}
			
			return Ok();
		}
		catch (StripeException e)
		{
			// Log the error for debugging
			System.Diagnostics.Debug.WriteLine($"Stripe webhook error: {e.Message}");
			return BadRequest();
		}
	}
}


