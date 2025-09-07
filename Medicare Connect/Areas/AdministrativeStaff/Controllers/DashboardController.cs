using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medicare_Connect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.AdministrativeStaff.Models;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff,Managers/Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalPatients = await (from u in _userManager.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where r.Name == "Patients"
                                   select u).CountAsync();
        var totalDoctors = await (from u in _userManager.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where r.Name == "Doctors"
                                  select u).CountAsync();

        var today = DateTime.Today;
        var apptCount = await _context.Appointments
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < today.AddDays(1))
            .CountAsync();

        var paymentsToday = await _context.Payments
            .Where(p => p.CreatedAt >= today)
            .ToListAsync();
        var paymentsAmount = paymentsToday.Sum(p => p.Amount);

        var outstandingBills = await _context.Billings
            .Where(b => b.PaymentStatus != "Paid")
            .CountAsync();

        var nextAppts = await _context.Appointments
            .Where(a => a.AppointmentDate >= DateTime.Today)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(5)
            .ToListAsync();

        var recentPayments = await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            UpcomingAppointments = apptCount,
            PaymentsTodayAmount = paymentsAmount,
            OutstandingBills = outstandingBills,
            NextAppointments = nextAppts,
            RecentPayments = recentPayments
        };

        return View(vm);
    }
}


