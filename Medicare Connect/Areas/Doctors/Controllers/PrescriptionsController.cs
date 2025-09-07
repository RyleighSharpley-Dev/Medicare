using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class PrescriptionsController : Controller
{
	private readonly UserManager<IdentityUser> _userManager;

	public PrescriptionsController(UserManager<IdentityUser> userManager)
	{
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var patients = await _userManager.GetUsersInRoleAsync("Patients");
		ViewBag.Patients = patients.Select(u => new { PatientKey = u.Id, Name = u.Email ?? u.UserName ?? u.Id }).OrderBy(x => x.Name).ToList();
		ViewBag.RefillRequests = PrescriptionStore.GetRefillRequestsForDoctor().ToList();
		return View(new List<Prescription>());
	}

	[HttpGet]
	public async Task<IActionResult> For(string patientKey)
	{
		var list = PrescriptionStore.GetPrescriptions(patientKey).OrderByDescending(p => p.Date).ToList();
		ViewBag.PatientKey = patientKey;
		// Repopulate patients list and refill requests just like Index
		var patients = await _userManager.GetUsersInRoleAsync("Patients");
		ViewBag.Patients = patients.Select(u => new { PatientKey = u.Id, Name = u.Email ?? u.UserName ?? u.Id }).OrderBy(x => x.Name).ToList();
		ViewBag.RefillRequests = PrescriptionStore.GetRefillRequestsForDoctor().ToList();
		return View("Index", list);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(string patientKey, PrescriptionCreateModel model)
	{
		if (string.IsNullOrWhiteSpace(patientKey) || !ModelState.IsValid)
		{
			TempData["RxError"] = "Please select a patient and complete the form.";
			return RedirectToAction("For", new { patientKey });
		}
		var doctor = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? "Doctor");
		var patientUser = await _userManager.FindByIdAsync(patientKey);
		var patientName = patientUser?.Email ?? patientUser?.UserName ?? patientKey;
		var p = new Prescription
		{
			PatientKey = patientKey,
			PatientName = patientName,
			Medication = model.Medication,
			Dosage = model.Dosage,
			PrescribedBy = doctor,
			Date = DateTime.UtcNow,
			Notes = model.Notes,
			RefillsTotal = model.RefillsTotal,
			RefillsUsed = 0,
			Status = PrescriptionStatus.Active
		};
		PrescriptionStore.AddPrescription(patientKey, p);
		TempData["RxSuccess"] = "Prescription created.";
		return RedirectToAction("For", new { patientKey });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult ApproveRefill(Guid id)
	{
		var r = PrescriptionStore.GetRefillRequestsForDoctor().FirstOrDefault(x => x.Id == id);
		if (r != null)
		{
			var doctor = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? "Doctor");
			PrescriptionStore.ApproveRefill(r, doctor);
			TempData["RxSuccess"] = "Refill approved.";
		}
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult RejectRefill(Guid id, string? notes)
	{
		var r = PrescriptionStore.GetRefillRequestsForDoctor().FirstOrDefault(x => x.Id == id);
		if (r != null)
		{
			var doctor = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? "Doctor");
			PrescriptionStore.RejectRefill(r, doctor, notes);
			TempData["RxSuccess"] = "Refill rejected.";
		}
		return RedirectToAction(nameof(Index));
	}
} 