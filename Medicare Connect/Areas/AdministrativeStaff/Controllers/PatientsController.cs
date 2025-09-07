using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.AdministrativeStaff.Models;
using System.Security.Claims;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class PatientsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public PatientsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        var patientList = patients.Select(u => new PatientListItem
        {
            Id = u.Id,
            Name = u.Email ?? u.UserName ?? u.Id,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber ?? "Not provided",
            RegistrationDate = DateTime.UtcNow
        }).OrderBy(p => p.Name).ToList();

        return View(patientList);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new PatientRegistrationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(PatientRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Create new user account
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, "TempPassword123!");

            if (result.Succeeded)
            {
                // Assign to Patients role
                await _userManager.AddToRoleAsync(user, "Patients");

                // Add custom claims for patient information
                var claims = new List<Claim>
                {
                    new Claim("FullName", $"{model.FirstName} {model.LastName}"),
                    new Claim("FirstName", model.FirstName),
                    new Claim("LastName", model.LastName),
                    new Claim("DateOfBirth", model.DateOfBirth.ToString("yyyy-MM-dd")),
                    new Claim("Gender", model.Gender),
                    new Claim("Address", model.Address),
                    new Claim("City", model.City),
                    new Claim("PostalCode", model.PostalCode)
                };

                if (!string.IsNullOrEmpty(model.EmergencyContactName))
                    claims.Add(new Claim("EmergencyContactName", model.EmergencyContactName));
                if (!string.IsNullOrEmpty(model.EmergencyContactPhone))
                    claims.Add(new Claim("EmergencyContactPhone", model.EmergencyContactPhone));
                if (!string.IsNullOrEmpty(model.InsuranceNumber))
                    claims.Add(new Claim("InsuranceNumber", model.InsuranceNumber));
                if (!string.IsNullOrEmpty(model.InsuranceProvider))
                    claims.Add(new Claim("InsuranceProvider", model.InsuranceProvider));

                await _userManager.AddClaimsAsync(user, claims);

                TempData["SuccessMessage"] = $"Patient {model.FirstName} {model.LastName} has been registered successfully. Temporary password: TempPassword123!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred during registration: {ex.Message}");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var patientInfo = new
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Unknown",
            FirstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? "Unknown",
            LastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? "Unknown",
            DateOfBirth = claims.FirstOrDefault(c => c.Type == "DateOfBirth")?.Value ?? "Unknown",
            Gender = claims.FirstOrDefault(c => c.Type == "Gender")?.Value ?? "Unknown",
            Address = claims.FirstOrDefault(c => c.Type == "Address")?.Value ?? "Unknown",
            City = claims.FirstOrDefault(c => c.Type == "City")?.Value ?? "Unknown",
            PostalCode = claims.FirstOrDefault(c => c.Type == "PostalCode")?.Value ?? "Unknown",
            EmergencyContactName = claims.FirstOrDefault(c => c.Type == "EmergencyContactName")?.Value,
            EmergencyContactPhone = claims.FirstOrDefault(c => c.Type == "EmergencyContactPhone")?.Value,
            InsuranceNumber = claims.FirstOrDefault(c => c.Type == "InsuranceNumber")?.Value,
            InsuranceProvider = claims.FirstOrDefault(c => c.Type == "InsuranceProvider")?.Value
        };

        return View(patientInfo);
    }
} 