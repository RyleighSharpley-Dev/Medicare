using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medicare_Connect.Controllers
{
    [Authorize]
    public class RoleRedirectController : Controller
    {
        public IActionResult RedirectToRoleDashboard()
        {
            // Get the user's roles from claims
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            // Redirect based on role priority
            if (userRoles.Contains("Managers/Admin") || userRoles.Contains("Managers"))
                return RedirectToAction("Index", "Dashboard", new { area = "Managers" });
            
            if (userRoles.Contains("Doctors"))
                return RedirectToAction("Index", "Dashboard", new { area = "Doctors" });
            
            if (userRoles.Contains("Nurses"))
                return RedirectToAction("Index", "Dashboard", new { area = "Nurses" });
            
            if (userRoles.Contains("Pharmacists"))
                return RedirectToAction("Index", "Dashboard", new { area = "Pharmacists" });
            
            if (userRoles.Contains("Administrative Staff"))
                return RedirectToAction("Index", "Dashboard", new { area = "AdministrativeStaff" });
            
            if (userRoles.Contains("Patients"))
                return RedirectToAction("Index", "Dashboard", new { area = "Patients" });

            // Default: If no specific role found, redirect to home page
            return RedirectToAction("Index", "Home");
        }
    }
}
