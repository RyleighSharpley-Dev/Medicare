using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Areas.Patients.Models;
using Stripe.Checkout;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class PrescriptionsController : Controller
{
	private readonly ApplicationDbContext _context;

	public PrescriptionsController(ApplicationDbContext context)
	{
		_context = context;
	}

	public IActionResult Index()
	{
		var key = PrescriptionStore.GetPatientKey(User);
		var list = PrescriptionStore.GetPrescriptions(key)
			.OrderByDescending(p => p.Date)
			.ToList();
		ViewBag.RefillRequests = PrescriptionStore.GetRefillRequestsForPatient(key).ToList();
		return View(list);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult RequestRefill(Guid prescriptionId)
	{
		var key = PrescriptionStore.GetPatientKey(User);
		var rx = PrescriptionStore.FindPrescription(prescriptionId);
		if (rx == null || rx.PatientKey != key)
		{
			TempData["RxError"] = "Prescription not found.";
			return RedirectToAction(nameof(Index));
		}
		if (rx.RefillsUsed >= rx.RefillsTotal)
		{
			TempData["RxError"] = "No refills remaining.";
			return RedirectToAction(nameof(Index));
		}
		PrescriptionStore.AddRefillRequest(new RefillRequest
		{
			PrescriptionId = rx.Id,
			PatientKey = key,
			Status = RefillRequestStatus.Pending
		});
		TempData["RxSuccess"] = "Refill requested. You can pay now or later from Payments.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> PayRefill(Guid refillRequestId)
	{
		var key = PrescriptionStore.GetPatientKey(User);
		var req = PrescriptionStore.GetRefillRequestsForPatient(key).FirstOrDefault(r => r.Id == refillRequestId);
		if (req == null)
		{
			TempData["RxError"] = "Refill request not found.";
			return RedirectToAction(nameof(Index));
		}
		if (req.IsPaid)
		{
			TempData["RxSuccess"] = "Refill already paid.";
			return RedirectToAction(nameof(Index));
		}
		var baseUrl = $"{Request.Scheme}://{Request.Host}";
		var options = new SessionCreateOptions
		{
			Mode = "payment",
			SuccessUrl = baseUrl + Url.Action("RefillPaid", "Payments", new { area = "Patients", refillRequestId = req.Id })!,
			CancelUrl = baseUrl + Url.Action("Index", "Prescriptions", new { area = "Patients" })!,
			LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					Quantity = 1,
					PriceData = new SessionLineItemPriceDataOptions
					{
						Currency = "zar",
						UnitAmount = req.PriceCents,
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = "Prescription Refill Fee",
							Description = $"Refill request {req.Id.ToString().Substring(0, 8)}"
						}
					}
				}
			}
		};
		var service = new SessionService();
		var session = service.Create(options);
		
		// Create pending payment record in database
		try
		{
			var payment = new PaymentEntity
			{
				PatientId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? key,
				PaymentType = PaymentType.PrescriptionRefill,
				Description = $"Prescription Refill - {req.PrescriptionId}",
				Amount = req.PriceCents / 100.0m,
				Status = PaymentStatus.Pending,
				StripeSessionId = session.Id,
				ReferenceId = req.Id.ToString(),
				Notes = "Prescription refill payment",
				CreatedAt = DateTime.UtcNow
			};

			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();
		}
		catch (Exception ex)
		{
			// Log error but don't fail the payment initiation flow
			System.Diagnostics.Debug.WriteLine($"Failed to save payment record: {ex.Message}");
		}
		
		return Redirect(session.Url);
	}
}


