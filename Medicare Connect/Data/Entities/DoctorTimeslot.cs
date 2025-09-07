using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Data.Entities
{
    public class DoctorTimeslot
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [Range(15, 480)] // 15 minutes to 8 hours
        public int DurationMinutes { get; set; } = 30;

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string? RecurrencePattern { get; set; } // "Daily", "Weekly", "Monthly"

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for doctor information
        public virtual IdentityUser? Doctor { get; set; }

        // Navigation property for appointments
        public virtual ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
    }
} 