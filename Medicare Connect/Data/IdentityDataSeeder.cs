using System.Security.Claims;
using Medicare_Connect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medicare_Connect.Data;

public static class IdentityDataSeeder
{
    private const string DefaultPassword = "Password123!";

    private static readonly Dictionary<string, string[]> RoleToResponsibilities = new()
    {
        ["Patients"] = new[]
        {
            "Create and maintain secure digital profiles with medical history",
            "Book, modify, or cancel appointments online",
            "View personal medical records and test results",
            "Access and review digital prescriptions",
            "Make online payments for services",
            "Receive appointment and medication reminders",
            "Update personal and contact information"
        },
        ["Doctors"] = new[]
        {
            "Access and review patient medical records",
            "Conduct digital consultations and update treatment plans",
            "Create and send electronic prescriptions to pharmacy",
            "Manage personal appointment schedules",
            "Add clinical notes and treatment documentation",
            "Review test results and patient history",
            "Communicate with other healthcare staff"
        },
        ["Nurses"] = new[]
        {
            "Assist with patient care coordination",
            "Update patient records with nursing notes",
            "Collaborate with doctors and other staff",
            "Manage patient flow and triage",
            "Access patient information for care delivery",
            "Coordinate follow-up care"
        },
        ["Pharmacists"] = new[]
        {
            "Receive and process electronic prescriptions",
            "Manage medication inventory and stock levels",
            "Document medication dispensing",
            "Track drug interactions and allergies",
            "Monitor prescription histories",
            "Maintain pharmacy records",
            "Handle medication refill requests"
        },
        ["Administrative Staff"] = new[]
        {
            "Register new patients in the system",
            "Manage appointment scheduling and confirmations",
            "Handle billing and payment processing",
            "Manage staff schedules and leave requests",
            "Generate reports and analytics",
            "Maintain system user accounts",
            "Handle patient inquiries and support"
        },
        ["Managers/Admin"] = new[]
        {
            "Oversee system operations and user management",
            "Access comprehensive analytics and reports",
            "Manage staff permissions and roles",
            "Monitor system performance and security",
            "Handle escalated issues",
            "Make strategic decisions based on system data",
            "Ensure compliance and audit trail maintenance"
        }
    };

    private static readonly (string Email, string FullName, string Role)[] SeedUsers = new[]
    {
        ("sibusiso@medicare.com",   "Sibusiso Dlamini",      "Patients"),
        ("thandeka@medicare.com",   "Thandeka Khumalo",      "Doctors"),
        ("nomvula@medicare.com",    "Nomvula Ndlovu",        "Nurses"),
        ("siyabonga@medicare.com",  "Siyabonga Maseko",      "Pharmacists"),
        ("zandile@medicare.com",    "Zandile Mthembu",       "Administrative Staff"),
        ("thembinkosi@medicare.com","Thembinkosi Mkhize",    "Managers/Admin"),
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        // Ensure roles exist and attach a description claim with responsibilities
        foreach (var (roleName, responsibilities) in RoleToResponsibilities)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var roleResult = await roleManager.CreateAsync(role);
                if (roleResult.Succeeded)
                {
                    var description = string.Join("; ", responsibilities);
                    await roleManager.AddClaimAsync(role, new Claim("Responsibilities", description));
                    await roleManager.AddClaimAsync(role, new Claim("Region", "South Africa"));
                }
            }
        }

        // Seed users with Zulu names and assign roles
        foreach (var (email, fullName, role) in SeedUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, DefaultPassword);
                if (!result.Succeeded)
                {
                    continue;
                }

                var names = fullName.Split(' ', 2);
                var given = names.Length > 0 ? names[0] : fullName;
                var family = names.Length > 1 ? names[1] : string.Empty;

                var claims = new List<Claim>
                {
                    new(ClaimTypes.GivenName, given),
                    new(ClaimTypes.Surname, family),
                    new("FullName", fullName),
                    new("Locale", "en-ZA"),
                    new("Culture", "South Africa")
                };
                await userManager.AddClaimsAsync(user, claims);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}


