using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Areas.AdministrativeStaff.Models;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class SchedulingController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public SchedulingController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? department, DateTime? startDate, DateTime? endDate)
    {
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
        var nurses = await _userManager.GetUsersInRoleAsync("Nurses");
        var adminStaff = await _userManager.GetUsersInRoleAsync("Administrative Staff");

        // Mock schedule data for demonstration
        var scheduleList = new List<StaffScheduleListItem>();
        var allStaff = doctors.Concat(nurses).Concat(adminStaff).ToList();

        foreach (var staff in allStaff.Take(15)) // Limit to first 15 for demo
        {
            var random = new Random(staff.Id.GetHashCode());
            var startDateValue = DateTime.Today.AddDays(random.Next(0, 7));
            var endDateValue = startDateValue.AddDays(random.Next(1, 5));
            
            scheduleList.Add(new StaffScheduleListItem
            {
                Id = Guid.NewGuid().ToString(),
                StaffName = staff.Email ?? staff.UserName ?? staff.Id,
                Department = GetRandomDepartment(random),
                ShiftType = GetRandomShiftType(random),
                StartDate = startDateValue,
                EndDate = endDateValue,
                StartTime = GetRandomStartTime(random),
                EndTime = GetRandomEndTime(random),
                Status = "Scheduled"
            });
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(department))
        {
            scheduleList = scheduleList.Where(s => s.Department.Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (startDate.HasValue)
        {
            scheduleList = scheduleList.Where(s => s.StartDate >= startDate.Value).ToList();
        }

        if (endDate.HasValue)
        {
            scheduleList = scheduleList.Where(s => s.EndDate <= endDate.Value).ToList();
        }

        scheduleList = scheduleList.OrderBy(s => s.StartDate).ThenBy(s => s.StartTime).ToList();

        ViewBag.Department = department;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.Staff = allStaff.Select(s => new { Id = s.Id, Name = s.Email ?? s.UserName ?? s.Id }).ToList();

        return View(scheduleList);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
        var nurses = await _userManager.GetUsersInRoleAsync("Nurses");
        var adminStaff = await _userManager.GetUsersInRoleAsync("Administrative Staff");
        var allStaff = doctors.Concat(nurses).Concat(adminStaff).ToList();

        ViewBag.Staff = allStaff.Select(s => new { Id = s.Id, Name = s.Email ?? s.UserName ?? s.Id }).ToList();

        return View(new StaffSchedulingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffSchedulingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
            var nurses = await _userManager.GetUsersInRoleAsync("Nurses");
            var adminStaff = await _userManager.GetUsersInRoleAsync("Administrative Staff");
            var allStaff = doctors.Concat(nurses).Concat(adminStaff).ToList();

            ViewBag.Staff = allStaff.Select(s => new { Id = s.Id, Name = s.Email ?? s.UserName ?? s.Id }).ToList();
            return View(model);
        }

        try
        {
            // Here you would typically save the schedule information to a database
            // For now, we'll just show a success message
            TempData["SuccessMessage"] = "Staff schedule created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            
            var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
            var nurses = await _userManager.GetUsersInRoleAsync("Nurses");
            var adminStaff = await _userManager.GetUsersInRoleAsync("Administrative Staff");
            var allStaff = doctors.Concat(nurses).Concat(adminStaff).ToList();

            ViewBag.Staff = allStaff.Select(s => new { Id = s.Id, Name = s.Email ?? s.UserName ?? s.Id }).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> LeaveRequests()
    {
        var doctors = await _userManager.GetUsersInRoleAsync("Doctors");
        var nurses = await _userManager.GetUsersInRoleAsync("Nurses");
        var adminStaff = await _userManager.GetUsersInRoleAsync("Administrative Staff");
        var allStaff = doctors.Concat(nurses).Concat(adminStaff).ToList();

        // Mock leave request data
        var leaveRequests = new List<LeaveRequestViewModel>();
        foreach (var staff in allStaff.Take(8))
        {
            var random = new Random(staff.Id.GetHashCode());
            var startDate = DateTime.Today.AddDays(random.Next(7, 30));
            var endDate = startDate.AddDays(random.Next(1, 5));
            
            leaveRequests.Add(new LeaveRequestViewModel
            {
                StaffId = staff.Id,
                LeaveType = GetRandomLeaveType(random),
                StartDate = startDate,
                EndDate = endDate,
                Reason = GetRandomLeaveReason(random),
                Status = GetRandomLeaveStatus(random)
            });
        }

        ViewBag.Staff = allStaff.Select(s => new { Id = s.Id, Name = s.Email ?? s.UserName ?? s.Id }).ToList();
        return View(leaveRequests);
    }

    private string GetRandomDepartment(Random random)
    {
        var departments = new[] { "Emergency Department", "ICU", "Cardiology", "Pediatrics", "Surgery", "Radiology", "Laboratory", "Administration" };
        return departments[random.Next(departments.Length)];
    }

    private string GetRandomShiftType(Random random)
    {
        var shifts = new[] { "Day Shift", "Evening Shift", "Night Shift", "Weekend Shift", "On-Call" };
        return shifts[random.Next(shifts.Length)];
    }

    private TimeSpan GetRandomStartTime(Random random)
    {
        var times = new[] { 6, 7, 8, 9, 14, 15, 16, 22, 23 };
        var hour = times[random.Next(times.Length)];
        return new TimeSpan(hour, 0, 0);
    }

    private TimeSpan GetRandomEndTime(Random random)
    {
        var times = new[] { 14, 15, 16, 17, 22, 23, 0, 6, 7 };
        var hour = times[random.Next(times.Length)];
        return new TimeSpan(hour, 0, 0);
    }

    private string GetRandomLeaveType(Random random)
    {
        var types = new[] { "Annual Leave", "Sick Leave", "Personal Leave", "Maternity/Paternity", "Study Leave", "Emergency Leave" };
        return types[random.Next(types.Length)];
    }

    private string GetRandomLeaveReason(Random random)
    {
        var reasons = new[] { "Family vacation", "Medical appointment", "Personal matters", "Training course", "Family emergency", "Rest and relaxation" };
        return reasons[random.Next(reasons.Length)];
    }

    private string GetRandomLeaveStatus(Random random)
    {
        var statuses = new[] { "Pending", "Approved", "Rejected", "Under Review" };
        return statuses[random.Next(statuses.Length)];
    }
} 