using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Doctors.Models;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Doctors.Controllers;

[Area("Doctors")]
[Authorize(Roles = "Doctors")]
public class ProfileController : Controller
{
	private readonly IWebHostEnvironment _webHostEnvironment;

	public ProfileController(IWebHostEnvironment webHostEnvironment)
	{
		_webHostEnvironment = webHostEnvironment;
	}

	[HttpGet]
	public IActionResult Index()
	{
		var model = new DoctorProfileViewModel
		{
			FullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? (User.Identity?.Name ?? string.Empty),
			Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? (User.Identity?.Name ?? string.Empty),
			PhoneNumber = null,
			Department = null,
			Specialty = null,
			OfficeLocation = null,
			Bio = null,
			ProfilePhotoPath = TempData["DoctorProfilePhotoPath"] as string ?? GetProfilePhotoPath(GetStableUserKey())
		};
		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Index(DoctorProfileViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View(model);
		}

		if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
		{
			var userId = GetStableUserKey();
			var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "doctor-profile-photos");
			if (!Directory.Exists(uploadsFolder))
			{
				Directory.CreateDirectory(uploadsFolder);
			}
			var uniqueFileName = $"{userId}{Path.GetExtension(model.ProfilePhoto.FileName)}";
			var filePath = Path.Combine(uploadsFolder, uniqueFileName);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
			using (var fs = new FileStream(filePath, FileMode.Create))
			{
				await model.ProfilePhoto.CopyToAsync(fs);
			}
			model.ProfilePhotoPath = $"/uploads/doctor-profile-photos/{uniqueFileName}";
		}

		TempData["DoctorStatusMessage"] = "Your profile has been updated.";
		TempData["DoctorProfilePhotoPath"] = model.ProfilePhotoPath;
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult RemoveProfilePhoto()
	{
		var userId = GetStableUserKey();
		var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "doctor-profile-photos");
		if (Directory.Exists(uploadsFolder))
		{
			var files = Directory.GetFiles(uploadsFolder)
				.Where(f => Path.GetFileNameWithoutExtension(f) == userId)
				.ToList();
			foreach (var file in files)
			{
				if (System.IO.File.Exists(file))
				{
					System.IO.File.Delete(file);
				}
			}
		}
		TempData["DoctorStatusMessage"] = "Profile photo removed.";
		return RedirectToAction(nameof(Index));
	}

	private string? GetProfilePhotoPath(string userId)
	{
		var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "doctor-profile-photos");
		if (!Directory.Exists(uploadsFolder))
		{
			return null;
		}
		var files = Directory.GetFiles(uploadsFolder)
			.Where(f => Path.GetFileNameWithoutExtension(f) == userId)
			.ToList();
		if (files.Any())
		{
			var fileName = Path.GetFileName(files.First());
			return $"/uploads/doctor-profile-photos/{fileName}";
		}
		return null;
	}

	private string GetStableUserKey()
	{
		var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!string.IsNullOrWhiteSpace(nameId))
		{
			return nameId;
		}
		var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
		if (!string.IsNullOrWhiteSpace(email))
		{
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_');
		}
		return HttpContext.Session.Id;
	}
} 