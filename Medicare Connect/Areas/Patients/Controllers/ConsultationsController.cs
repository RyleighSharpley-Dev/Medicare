using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using System.Security.Claims;
using Medicare_Connect.Data;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class ConsultationsController : Controller
{
	private readonly ApplicationDbContext _db;
	public ConsultationsController(ApplicationDbContext db)
	{
		_db = db;
	}

	[HttpGet]
	public IActionResult Index()
	{
		var patientKey = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? (User.Identity?.Name ?? string.Empty);
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
		return View(list);
	}
} 