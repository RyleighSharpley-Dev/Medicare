using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Medicare_Connect.Data
{
    public static class TimeslotDataSeeder
    {
        public static async Task SeedTimeslotsAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

            // Get doctors
            var doctors = await userManager.GetUsersInRoleAsync("Doctors");
            if (!doctors.Any())
                return;

            // Check if timeslots already exist
            if (await db.DoctorTimeslots.AnyAsync())
                return;

            var timeslots = new List<DoctorTimeslot>();

            foreach (var doctor in doctors)
            {
                // Create timeslots for the next 30 days
                for (int day = 0; day < 30; day++)
                {
                    var date = DateTime.Today.AddDays(day);
                    
                    // Skip weekends (Saturday = 6, Sunday = 0)
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Create morning timeslots (9 AM to 12 PM)
                    for (int hour = 9; hour < 12; hour++)
                    {
                        for (int minute = 0; minute < 60; minute += 30)
                        {
                            var startTime = date.AddHours(hour).AddMinutes(minute);
                            var endTime = startTime.AddMinutes(30);

                            timeslots.Add(new DoctorTimeslot
                            {
                                DoctorId = doctor.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                DurationMinutes = 30,
                                IsAvailable = true,
                                IsRecurring = false,
                                Notes = "Standard appointment slot"
                            });
                        }
                    }

                    // Create afternoon timeslots (2 PM to 5 PM)
                    for (int hour = 14; hour < 17; hour++)
                    {
                        for (int minute = 0; minute < 60; minute += 30)
                        {
                            var startTime = date.AddHours(hour).AddMinutes(minute);
                            var endTime = startTime.AddMinutes(30);

                            timeslots.Add(new DoctorTimeslot
                            {
                                DoctorId = doctor.Id,
                                StartTime = startTime,
                                EndTime = endTime,
                                DurationMinutes = 30,
                                IsAvailable = true,
                                IsRecurring = false,
                                Notes = "Standard appointment slot"
                            });
                        }
                    }
                }
            }

            if (timeslots.Any())
            {
                await db.DoctorTimeslots.AddRangeAsync(timeslots);
                await db.SaveChangesAsync();
            }
        }
    }
} 