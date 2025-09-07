using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class RecordsController : Controller
{
	private readonly UserManager<IdentityUser> _userManager;

	public RecordsController(UserManager<IdentityUser> userManager)
	{
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		// Doctors can select a patient and view their records
		var patientUsers = await _userManager.GetUsersInRoleAsync("Patients");
		var patients = new List<object>();
		foreach (var u in patientUsers)
		{
			var name = u.Email ?? u.UserName ?? u.Id;
			patients.Add(new { PatientKey = u.Id, Name = name });
		}
		ViewBag.Patients = patients.OrderBy(x => ((dynamic)x).Name).ToList();
		return View(new List<PatientRecord>());
	}

	[HttpGet]
	public async Task<IActionResult> For(string patientKey)
	{
		var records = RecordStore.GetList(patientKey).OrderByDescending(r => r.Date).ToList();
		ViewBag.PatientKey = patientKey;
		var user = await _userManager.FindByIdAsync(patientKey);
		ViewBag.PatientName = user?.Email ?? user?.UserName ?? patientKey;
		return View("Index", records);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Upload(string patientKey, RecordCreateModel model)
	{
		if (string.IsNullOrWhiteSpace(patientKey))
		{
			TempData["RecError"] = "Select a valid patient.";
			return RedirectToAction(nameof(Index));
		}

		var targetUser = await _userManager.FindByIdAsync(patientKey);
		if (targetUser == null || !(await _userManager.IsInRoleAsync(targetUser, "Patients")))
		{
			TempData["RecError"] = "Patient not found.";
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			TempData["RecError"] = "Please complete the form.";
			return RedirectToAction("For", new { patientKey });
		}

		RecordStore.Add(patientKey, new PatientRecord
		{
			PatientKey = patientKey,
			Title = model.Title,
			Type = model.Type,
			Date = model.Date ?? DateTime.UtcNow,
			Notes = model.Notes,
			FileName = null,
			ContentType = null,
			FileSizeBytes = null,
			RelativePath = null
		});
		TempData["RecSuccess"] = "Record saved for patient.";
		return RedirectToAction("For", new { patientKey });
	}
} 