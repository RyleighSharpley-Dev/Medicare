using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.AdministrativeStaff.Models;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class ReportsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public ReportsController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
        var nurses = await _userManager.GetUsersInRoleAsync("Nurses");

        var model = new DashboardMetricsViewModel
        {
            TotalPatients = patients.Count,
            TotalDoctors = doctors.Count,
            TotalNurses = nurses.Count,
            TotalAppointments = 0, // Would come from appointment store
            PendingAppointments = 0,
            CompletedAppointments = 0,
            TotalRevenue = 0,
            PendingPayments = 0,
            OverdueBills = 0,
            RecentActivities = GetMockRecentActivities()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Generate()
    {
        return View(new ReportRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(ReportRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Here you would typically generate the actual report
            // For now, we'll create a mock report summary
            var reportSummary = new ReportSummaryViewModel
            {
                ReportType = model.ReportType,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalRecords = new Random().Next(100, 1000),
                TotalRevenue = new Random().Next(50000, 500000) / 100.0m,
                TotalPatients = new Random().Next(50, 200),
                TotalAppointments = new Random().Next(200, 800),
                Department = model.Department ?? "All Departments",
                ChartData = GenerateMockChartData()
            };

            TempData["ReportGenerated"] = true;
            TempData["ReportSummary"] = reportSummary;

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred while generating the report: {ex.Message}");
            return View(model);
        }
    }

    private List<RecentActivity> GetMockRecentActivities()
    {
        return new List<RecentActivity>
        {
            new RecentActivity { Description = "New patient registered", Timestamp = DateTime.Now.AddHours(-1), Type = "Registration", User = "Admin Staff" },
            new RecentActivity { Description = "Appointment scheduled", Timestamp = DateTime.Now.AddHours(-2), Type = "Scheduling", User = "Admin Staff" },
            new RecentActivity { Description = "Payment received", Timestamp = DateTime.Now.AddHours(-3), Type = "Payment", User = "System" },
            new RecentActivity { Description = "Staff schedule updated", Timestamp = DateTime.Now.AddHours(-4), Type = "Scheduling", User = "Admin Staff" },
            new RecentActivity { Description = "Report generated", Timestamp = DateTime.Now.AddHours(-5), Type = "Reports", User = "Admin Staff" }
        };
    }

    private List<ChartDataPoint> GenerateMockChartData()
    {
        var random = new Random();
        return new List<ChartDataPoint>
        {
            new ChartDataPoint { Label = "General Checkup", Value = random.Next(1000, 5000) / 100.0m, Color = "#007bff" },
            new ChartDataPoint { Label = "Follow-up", Value = random.Next(500, 2000) / 100.0m, Color = "#28a745" },
            new ChartDataPoint { Label = "Lab Results", Value = random.Next(300, 1500) / 100.0m, Color = "#ffc107" },
            new ChartDataPoint { Label = "Vaccination", Value = random.Next(200, 1000) / 100.0m, Color = "#dc3545" },
            new ChartDataPoint { Label = "Specialist", Value = random.Next(800, 3000) / 100.0m, Color = "#6f42c1" }
        };
    }
} 