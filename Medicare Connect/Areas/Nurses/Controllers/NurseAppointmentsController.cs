using Medicare_Connect.Areas.Patients.Services;
using Medicare_Connect.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medicare_Connect.Areas.Nurses.Controllers
{
    [Area("Nurses")]
    public class NurseAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITimeslotService _timeslotService;

        public NurseAppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ITimeslotService timeslotService)
        {
            _context = context;
            _userManager = userManager;
            _timeslotService = timeslotService;
        }
       
        [HttpGet]
        public async Task<IActionResult> Index(string patientKey)
        {
            if (string.IsNullOrEmpty(patientKey))
            {
                return BadRequest("Patient key is required.");
            }

            var patient = _context.Users.Find(patientKey);
            if(patient == null)
            {
                return BadRequest("Patient not found.");
            }

            var appts = await _context.Appointments
                .Where(a => a.PatientId == patientKey)
                .Include(a => a.Patient)
                .Include(a => a.Timeslot)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewData["PatientKey"] = patientKey;
            ViewData["PatientEmail"] = patient.Email ?? "Unknown";

            return View(appts);
        }
    }
}
