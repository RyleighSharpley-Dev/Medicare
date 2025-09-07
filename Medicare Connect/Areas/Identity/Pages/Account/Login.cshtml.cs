using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Medicare_Connect.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginModel(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Redirect to role dashboard using our RoleRedirect controller
            return LocalRedirect("/RoleRedirect/RedirectToRoleDashboard");
        }
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "User account locked out.");
            ReturnUrl = returnUrl;
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        ReturnUrl = returnUrl;
        return Page();
    }
}


