using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Doctors.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class PatientsController : Controller
{
	private readonly UserManager<IdentityUser> _userManager;

	public PatientsController(UserManager<IdentityUser> userManager)
	{
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IActionResult> Index(string? q)
	{
		var qLower = (q ?? string.Empty).Trim().ToLowerInvariant();
		var users = await _userManager.GetUsersInRoleAsync("Patients");
		var allByPatient = AppointmentStore.GetAllByPatientKey();

		var list = new List<DoctorPatientViewModel>();
		foreach (var u in users)
		{
			var name = u.Email ?? u.UserName ?? u.Id;
			if (!string.IsNullOrEmpty(qLower) && !(name.ToLowerInvariant().Contains(qLower) || u.Id.ToLowerInvariant().Contains(qLower)))
			{
				continue;
			}
			var key = u.Id;
			var appts = allByPatient.ContainsKey(key) ? allByPatient[key] : new List<Appointment>();
			var next = appts.Where(x => x.AppointmentDate >= DateTime.Now && x.Status == AppointmentStatus.Booked)
							.OrderBy(x => x.AppointmentDate)
							.FirstOrDefault()?.AppointmentDate;
			var last = appts.Where(x => x.AppointmentDate < DateTime.Now)
						  .OrderByDescending(x => x.AppointmentDate)
						  .FirstOrDefault()?.AppointmentDate;
			list.Add(new DoctorPatientViewModel
			{
				PatientKey = key,
				DisplayName = name,
				TotalAppointments = appts.Count,
				NextAppointment = next,
				LastAppointment = last
			});
		}

		list = list.OrderBy(x => x.DisplayName).ToList();
		ViewBag.Query = q ?? string.Empty;
		return View(list);
	}
}


