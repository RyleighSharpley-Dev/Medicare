using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.Patients.Models;
using System.IO;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class ProfileController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProfileController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        // In a real application, this data would come from a database
        // For now, we'll create a sample profile with the current user's information
        var userKey = GetStableUserKey();
        // Prefer a freshly uploaded path if available
        var profilePhotoPath = TempData["ProfilePhotoPath"] as string ?? GetProfilePhotoPath(userKey);

        var profile = new PatientProfileViewModel
        {
            FullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? User.Identity?.Name ?? string.Empty,
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? string.Empty,
            DateOfBirth = new DateTime(1985, 5, 15),
            IdNumber = "8505151234567",
            Gender = "Male",
            PhoneNumber = "071 234 5678",
            Address = "123 Mandela Street",
            City = "Johannesburg",
            Province = "Gauteng",
            PostalCode = "2000",
            EmergencyContactName = "Thabo Dlamini",
            EmergencyContactRelationship = "Brother",
            EmergencyContactPhone = "082 345 6789",
            BloodType = "O+",
            Allergies = "Penicillin, Peanuts",
            ChronicConditions = "Hypertension",
            CurrentMedications = "Lisinopril 10mg daily",
            MedicalAidScheme = "Discovery Health",
            MedicalAidNumber = "12345678",
            MainMemberName = "Sibusiso Dlamini",
            DependantCode = "00",
            ProfilePhotoPath = profilePhotoPath
        };

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PatientProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Handle profile photo upload
        if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
        {
            var userId = GetStableUserKey();
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-photos");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            
            // Generate a unique file name
            var uniqueFileName = $"{userId}{Path.GetExtension(model.ProfilePhoto.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            // Delete existing file if it exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            
            // Save the new file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePhoto.CopyToAsync(fileStream);
            }
            
            // Update the model with the new file path
            model.ProfilePhotoPath = $"/uploads/profile-photos/{uniqueFileName}";
        }

        // In a real application, we would save the profile to a database here
        // For now, we'll just redirect back with a success message

        TempData["StatusMessage"] = "Your profile has been updated successfully.";
        TempData["ProfilePhotoPath"] = model.ProfilePhotoPath;
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveProfilePhoto()
    {
        var userId = GetStableUserKey();
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-photos");
        
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
        
        TempData["StatusMessage"] = "Profile photo removed successfully.";
        return RedirectToAction(nameof(Index));
    }
    
    private string? GetProfilePhotoPath(string userId)
    {
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-photos");
        
        if (!Directory.Exists(uploadsFolder))
        {
            return null;
        }
        
        // Check for any file with the userId as the filename (regardless of extension)
        var files = Directory.GetFiles(uploadsFolder)
            .Where(f => Path.GetFileNameWithoutExtension(f) == userId)
            .ToList();
            
        if (files.Any())
        {
            var fileName = Path.GetFileName(files.First());
            return $"/uploads/profile-photos/{fileName}";
        }
        
        return null;
    }

    private string GetStableUserKey()
    {
        // Prefer the ASP.NET Identity user id
        var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(nameId))
        {
            return nameId;
        }

        // Fallback to email
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(email))
        {
            // Make a filesystem-safe key from email
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        // Last resort: a deterministic value per session (not ideal, but avoids random GUIDs)
        return HttpContext.Session.Id;
    }
}


