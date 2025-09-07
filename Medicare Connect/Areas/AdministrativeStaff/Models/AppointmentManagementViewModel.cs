using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class AppointmentManagementViewModel
    {
        [Required]
        [Display(Name = "Patient")]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Doctor")]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Appointment Type")]
        public string AppointmentType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        [Display(Name = "Duration (minutes)")]
        [Range(15, 240)]
        public int DurationMinutes { get; set; } = 30;

        [Display(Name = "Location")]
        [StringLength(100)]
        public string? Location { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Insurance Coverage")]
        public bool InsuranceCoverage { get; set; } = false;

        [Display(Name = "Requires Pre-authorization")]
        public bool RequiresPreAuthorization { get; set; } = false;

        [Display(Name = "Priority Level")]
        public string PriorityLevel { get; set; } = "Normal";
    }

    public class AppointmentListItem
    {
        public string Id { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string AppointmentType { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
        public decimal? Price { get; set; }
    }
} 