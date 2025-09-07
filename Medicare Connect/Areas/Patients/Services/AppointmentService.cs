using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.Patients.Services
{
    public interface IAppointmentService
    {
        Task<List<AppointmentEntity>> GetPatientAppointmentsAsync(string patientId);
        Task<List<AppointmentEntity>> GetDoctorAppointmentsAsync(string doctorId, DateTime? date = null);
        Task<AppointmentEntity?> GetAppointmentByIdAsync(Guid appointmentId);
        Task<bool> CreateAppointmentAsync(AppointmentEntity appointment);
        Task<bool> UpdateAppointmentAsync(AppointmentEntity appointment);
        Task<bool> CancelAppointmentAsync(Guid appointmentId, string cancellationReason);
        Task<bool> ConfirmAppointmentAsync(Guid appointmentId);
        Task<bool> CompleteAppointmentAsync(Guid appointmentId);
        Task<bool> CheckForConflictsAsync(string doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeAppointmentId = null);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimeslotService _timeslotService;

        public AppointmentService(ApplicationDbContext context, ITimeslotService timeslotService)
        {
            _context = context;
            _timeslotService = timeslotService;
        }

        public async Task<List<AppointmentEntity>> GetPatientAppointmentsAsync(string patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Timeslot)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<List<AppointmentEntity>> GetDoctorAppointmentsAsync(string doctorId, DateTime? date = null)
        {
            IQueryable<AppointmentEntity> query = _context.Appointments
                .Where(a => a.DoctorId == doctorId);

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(a => a.AppointmentDate >= startOfDay && a.AppointmentDate < endOfDay);
            }

            return await query
                .Include(a => a.Patient)
                .Include(a => a.Timeslot)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<AppointmentEntity?> GetAppointmentByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Timeslot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task<bool> CreateAppointmentAsync(AppointmentEntity appointment)
        {
            try
            {
                // Check for conflicts before creating
                if (await CheckForConflictsAsync(appointment.DoctorId, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime))
                {
                    return false;
                }

                // Book the timeslot
                if (!await _timeslotService.BookTimeslotAsync(appointment.TimeslotId, appointment.PatientId))
                {
                    return false;
                }

                appointment.CreatedAt = DateTime.UtcNow;
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAppointmentAsync(AppointmentEntity appointment)
        {
            try
            {
                var existingAppointment = await _context.Appointments.FindAsync(appointment.Id);
                if (existingAppointment == null)
                    return false;

                // Check for conflicts if time/date changed
                if (existingAppointment.AppointmentDate != appointment.AppointmentDate ||
                    existingAppointment.StartTime != appointment.StartTime ||
                    existingAppointment.EndTime != appointment.EndTime)
                {
                    if (await CheckForConflictsAsync(appointment.DoctorId, appointment.AppointmentDate, appointment.StartTime, appointment.EndTime, appointment.Id))
                    {
                        return false;
                    }
                }

                appointment.UpdatedAt = DateTime.UtcNow;
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, string cancellationReason)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            try
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.CancellationReason = cancellationReason;
                appointment.UpdatedAt = DateTime.UtcNow;

                // Release the timeslot
                await _timeslotService.ReleaseTimeslotAsync(appointment.TimeslotId);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ConfirmAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            try
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.ConfirmedAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CompleteAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            try
            {
                appointment.Status = AppointmentStatus.Completed;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckForConflictsAsync(string doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate == appointmentDate &&
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.NoShow);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            var conflictingAppointments = await query
                .Where(a => (a.StartTime < endTime && a.EndTime > startTime))
                .ToListAsync();

            return conflictingAppointments.Any();
        }
    }
} 