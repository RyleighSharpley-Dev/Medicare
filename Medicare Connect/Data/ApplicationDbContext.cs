using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<ConsultationEntity> Consultations => Set<ConsultationEntity>();
		public DbSet<BillingEntity> Billings => Set<BillingEntity>();
		public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
		public DbSet<DoctorTimeslot> DoctorTimeslots => Set<DoctorTimeslot>();
		public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Configure relationships
			builder.Entity<DoctorTimeslot>()
				.HasOne(dt => dt.Doctor)
				.WithMany()
				.HasForeignKey(dt => dt.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AppointmentEntity>()
				.HasOne(a => a.Patient)
				.WithMany()
				.HasForeignKey(a => a.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AppointmentEntity>()
				.HasOne(a => a.Doctor)
				.WithMany()
				.HasForeignKey(a => a.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AppointmentEntity>()
				.HasOne(a => a.Timeslot)
				.WithMany(dt => dt.Appointments)
				.HasForeignKey(a => a.TimeslotId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure indexes for better performance
			builder.Entity<DoctorTimeslot>()
				.HasIndex(dt => new { dt.DoctorId, dt.StartTime, dt.EndTime });

			builder.Entity<AppointmentEntity>()
				.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime });

			builder.Entity<AppointmentEntity>()
				.HasIndex(a => new { a.PatientId, a.AppointmentDate });
		}
	}
}
