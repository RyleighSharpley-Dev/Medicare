using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.AdministrativeStaff.Models;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers
{
    [Area("AdministrativeStaff")]
    [Authorize(Roles = "Administrative Staff,Managers/Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.Take(500).ToListAsync();
            var items = new List<UserListItem>(users.Count);
            foreach (var u in users)
            {
                var r = await _userManager.GetRolesAsync(u);
                items.Add(new UserListItem { User = u, Roles = r.ToList() });
            }
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Roles"] = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string fullName, string[] roles)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["MsgError"] = "Email and password are required.";
                return RedirectToAction(nameof(Index));
            }
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["MsgError"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));
            }
            if (roles != null && roles.Length > 0)
            {
                foreach (var role in roles)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }
            }
            TempData["Msg"] = "User created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewData["User"] = user;
            ViewData["UserRoles"] = userRoles;
            ViewData["AllRoles"] = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, string email, string fullName, string[] roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["MsgError"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
            {
                user.Email = email; user.UserName = email;
            }
            var claims = await _userManager.GetClaimsAsync(user);
            var fullNameClaim = claims.FirstOrDefault(c => c.Type == "FullName");
            if (fullNameClaim != null) await _userManager.RemoveClaimAsync(user, fullNameClaim);
            if (!string.IsNullOrWhiteSpace(fullName)) await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));

            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Where(r => roles == null || !roles.Contains(r)).ToList();
            var toAdd = (roles ?? Array.Empty<string>()).Where(r => !currentRoles.Contains(r)).ToList();
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (toAdd.Any()) await _userManager.AddToRolesAsync(user, toAdd);

            await _userManager.UpdateAsync(user);
            TempData["Msg"] = "User updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["MsgError"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            TempData["Msg"] = result.Succeeded ? "Password reset." : string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["MsgError"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            var result = await _userManager.DeleteAsync(user);
            TempData["Msg"] = result.Succeeded ? "User deleted." : string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }
    }
} 