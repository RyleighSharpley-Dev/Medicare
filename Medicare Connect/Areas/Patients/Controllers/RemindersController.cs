using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class RemindersController : Controller
{
	[HttpGet]
	public IActionResult Index()
	{
		var key = ReminderStore.GetPatientKey(User);
		var list = ReminderStore.GetList(key)
			.OrderBy(r => r.IsCompleted)
			.ThenBy(r => r.DueAt ?? DateTime.MaxValue)
			.ToList();
		return View(list);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Create(ReminderCreateModel model)
	{
		if (!ModelState.IsValid)
		{
			TempData["RemError"] = "Please correct the errors and try again.";
			return RedirectToAction(nameof(Index));
		}
		var key = ReminderStore.GetPatientKey(User);
		ReminderStore.Add(key, new Reminder
		{
			PatientKey = key,
			Title = model.Title,
			Type = model.Type,
			DueAt = model.DueAt,
			Notes = model.Notes
		});
		TempData["RemSuccess"] = "Reminder created.";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Toggle(Guid id)
	{
		var key = ReminderStore.GetPatientKey(User);
		var r = ReminderStore.Find(key, id);
		if (r != null)
		{
			r.IsCompleted = !r.IsCompleted;
			TempData["RemSuccess"] = r.IsCompleted ? "Marked as completed." : "Marked as pending.";
		}
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Delete(Guid id)
	{
		var key = ReminderStore.GetPatientKey(User);
		if (ReminderStore.Remove(key, id))
		{
			TempData["RemSuccess"] = "Reminder deleted.";
		}
		return RedirectToAction(nameof(Index));
	}
}


