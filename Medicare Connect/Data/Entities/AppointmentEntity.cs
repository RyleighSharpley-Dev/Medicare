using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Data.Entities
{
    public enum AppointmentStatus
    {
        Booked = 0,
        Confirmed = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }

    public class AppointmentEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public Guid TimeslotId { get; set; }

        [Required]
        [StringLength(100)]
        public string AppointmentType { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(15, 480)]
        public int DurationMinutes { get; set; } = 30;

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

        [StringLength(100)]
        public string? Location { get; set; } = "Main Clinic";

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? PatientNotes { get; set; }

        [StringLength(500)]
        public string? DoctorNotes { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? InsuranceAmount { get; set; }

        [Required]
        public decimal PatientResponsibility { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(100)]
        public string? StripeSessionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // Navigation properties
        public virtual IdentityUser? Patient { get; set; }
        public virtual IdentityUser? Doctor { get; set; }
        public virtual DoctorTimeslot? Timeslot { get; set; }
    }
} 