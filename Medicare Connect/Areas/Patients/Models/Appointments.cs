using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
    public enum AppointmentStatus
    {
        Booked,
        Cancelled,
        Completed
    }

    public class Appointment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PatientKey { get; set; } = string.Empty;
        public string PatientDisplayName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentType { get; set; } = string.Empty; // e.g., General, Follow-up, Lab
        public string DoctorName { get; set; } = string.Empty;
        public string Location { get; set; } = "Main Clinic";
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

        // Payment info (demo, in-memory)
        public long PriceCents { get; set; }
        public bool IsPaid { get; set; }
        public string? StripeSessionId { get; set; }
    }

    public class AppointmentCreateModel
    {
        [Required]
        [Display(Name = "Date & Time")]
        public DateTime? AppointmentDate { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Appointment Type")]
        public string AppointmentType { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(128)]
        public string? Notes { get; set; }
    }

    public class AppointmentEditModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Date & Time")]
        public DateTime? AppointmentDate { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Appointment Type")]
        public string AppointmentType { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        [Display(Name = "Doctor")]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(128)]
        public string? Notes { get; set; }
    }
}


