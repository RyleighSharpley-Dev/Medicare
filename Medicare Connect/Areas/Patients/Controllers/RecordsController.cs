using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Areas.Patients.Services;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class RecordsController : Controller
{
	private readonly IWebHostEnvironment _env;

	public RecordsController(IWebHostEnvironment env)
	{
		_env = env;
	}

	[HttpGet]
	public IActionResult Index()
	{
		var key = RecordStore.GetPatientKey(User);
		var list = RecordStore.GetList(key)
			.OrderByDescending(r => r.Date)
			.ToList();
		return View(list);
	}

	[HttpGet]
	public IActionResult Download(Guid id)
	{
		var key = RecordStore.GetPatientKey(User);
		var rec = RecordStore.Find(key, id);
		if (rec == null)
		{
			return NotFound();
		}
		var abs = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), rec.RelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
		if (!System.IO.File.Exists(abs)) return NotFound();
		var bytes = System.IO.File.ReadAllBytes(abs);
		return File(bytes, rec.ContentType, rec.FileName);
	}
}


