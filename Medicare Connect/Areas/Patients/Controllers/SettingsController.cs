using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class SettingsController : Controller
{
	[HttpGet]
	public IActionResult Index()
	{
		var key = PatientSettingsStore.GetPatientKey(User);
		var model = PatientSettingsStore.Get(key);
		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Index(SettingsViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}
		var key = PatientSettingsStore.GetPatientKey(User);
		PatientSettingsStore.Save(key, model);
		TempData["SettingsSaved"] = true;
		return RedirectToAction(nameof(Index));
	}
}


