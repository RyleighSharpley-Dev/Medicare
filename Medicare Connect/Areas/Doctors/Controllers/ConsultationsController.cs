using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class ConsultationsController : Controller
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly ApplicationDbContext _db;

	public ConsultationsController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
	{
		_userManager = userManager;
		_db = db;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var patients = await _userManager.GetUsersInRoleAsync("Patients");
		ViewBag.Patients = patients.Select(u => new { PatientKey = u.Id, Name = u.Email ?? u.UserName ?? u.Id }).OrderBy(x => x.Name).ToList();
		return View(new List<Consultation>());
	}

	[HttpGet]
	public async Task<IActionResult> For(string patientKey)
	{
		var list = _db.Consultations
			.Where(c => c.PatientKey == patientKey)
			.OrderByDescending(c => c.WhenUtc)
			.Select(c => new Consultation
			{
				Id = c.Id,
				PatientKey = c.PatientKey,
				PatientName = c.PatientName,
				DoctorName = c.DoctorName,
				Subject = c.Subject,
				WhenUtc = c.WhenUtc,
				DurationMinutes = c.DurationMinutes,
				Notes = c.Notes,
				Status = (ConsultationStatus)c.Status
			})
			.ToList();
		ViewBag.PatientKey = patientKey;
		var patients = await _userManager.GetUsersInRoleAsync("Patients");
		ViewBag.Patients = patients.Select(u => new { PatientKey = u.Id, Name = u.Email ?? u.UserName ?? u.Id }).OrderBy(x => x.Name).ToList();
		return View("Index", list);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(string patientKey, ConsultationCreateModel model)
	{
		if (string.IsNullOrWhiteSpace(patientKey) || !ModelState.IsValid)
		{
			TempData["CError"] = "Select a patient and complete the form.";
			return RedirectToAction("For", new { patientKey });
		}
		var doctor = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? "Doctor");
		var u = await _userManager.FindByIdAsync(patientKey);
		var patientName = u?.Email ?? u?.UserName ?? patientKey;

		var entity = new ConsultationEntity
		{
			Id = Guid.NewGuid(),
			PatientKey = patientKey,
			PatientName = patientName!,
			DoctorName = doctor,
			Subject = model.Subject,
			WhenUtc = model.WhenUtc ?? DateTime.UtcNow,
			DurationMinutes = model.DurationMinutes,
			Notes = model.Notes,
			Status = (int)ConsultationStatus.Planned
		};
		_db.Consultations.Add(entity);
		await _db.SaveChangesAsync();

		TempData["CSuccess"] = "Consultation created.";
		return RedirectToAction("For", new { patientKey });
	}
} 