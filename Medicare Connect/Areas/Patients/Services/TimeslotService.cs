using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.Patients.Services
{
    public interface ITimeslotService
    {
        Task<List<DoctorTimeslot>> GetAvailableTimeslotsAsync(string doctorId, DateTime date);
        Task<bool> IsTimeslotAvailableAsync(Guid timeslotId);
        Task<bool> BookTimeslotAsync(Guid timeslotId, string patientId);
        Task<bool> ReleaseTimeslotAsync(Guid timeslotId);
        Task<List<DoctorTimeslot>> GetDoctorScheduleAsync(string doctorId, DateTime startDate, DateTime endDate);
        Task<bool> CreateTimeslotAsync(DoctorTimeslot timeslot);
        Task<bool> UpdateTimeslotAsync(DoctorTimeslot timeslot);
        Task<bool> DeleteTimeslotAsync(Guid timeslotId);
    }

    public class TimeslotService : ITimeslotService
    {
        private readonly ApplicationDbContext _context;

        public TimeslotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorTimeslot>> GetAvailableTimeslotsAsync(string doctorId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.DoctorTimeslots
                .Where(dt => dt.DoctorId == doctorId &&
                           dt.StartTime >= startOfDay &&
                           dt.StartTime < endOfDay &&
                           dt.IsAvailable &&
                           !dt.Appointments.Any(a => a.Status != AppointmentStatus.Cancelled))
                .OrderBy(dt => dt.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeslotAvailableAsync(Guid timeslotId)
        {
            var timeslot = await _context.DoctorTimeslots
                .Include(dt => dt.Appointments)
                .FirstOrDefaultAsync(dt => dt.Id == timeslotId);

            if (timeslot == null || !timeslot.IsAvailable)
                return false;

            // Check if there are any active appointments for this timeslot
            return !timeslot.Appointments.Any(a => a.Status != AppointmentStatus.Cancelled);
        }

        public async Task<bool> BookTimeslotAsync(Guid timeslotId, string patientId)
        {
            if (!await IsTimeslotAvailableAsync(timeslotId))
                return false;

            var timeslot = await _context.DoctorTimeslots.FindAsync(timeslotId);
            if (timeslot == null)
                return false;

            // Mark timeslot as unavailable
            timeslot.IsAvailable = false;
            timeslot.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReleaseTimeslotAsync(Guid timeslotId)
        {
            var timeslot = await _context.DoctorTimeslots.FindAsync(timeslotId);
            if (timeslot == null)
                return false;

            timeslot.IsAvailable = true;
            timeslot.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DoctorTimeslot>> GetDoctorScheduleAsync(string doctorId, DateTime startDate, DateTime endDate)
        {
            return await _context.DoctorTimeslots
                .Where(dt => dt.DoctorId == doctorId &&
                           dt.StartTime >= startDate &&
                           dt.StartTime <= endDate)
                .Include(dt => dt.Appointments)
                .OrderBy(dt => dt.StartTime)
                .ToListAsync();
        }

        public async Task<bool> CreateTimeslotAsync(DoctorTimeslot timeslot)
        {
            try
            {
                _context.DoctorTimeslots.Add(timeslot);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTimeslotAsync(DoctorTimeslot timeslot)
        {
            try
            {
                timeslot.UpdatedAt = DateTime.UtcNow;
                _context.DoctorTimeslots.Update(timeslot);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTimeslotAsync(Guid timeslotId)
        {
            var timeslot = await _context.DoctorTimeslots.FindAsync(timeslotId);
            if (timeslot == null)
                return false;

            try
            {
                _context.DoctorTimeslots.Remove(timeslot);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 