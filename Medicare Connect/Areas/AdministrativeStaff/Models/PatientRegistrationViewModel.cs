using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class PatientRegistrationViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "City")]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Emergency Contact Name")]
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        [Phone]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "Medical Insurance Number")]
        [StringLength(50)]
        public string? InsuranceNumber { get; set; }

        [Display(Name = "Insurance Provider")]
        [StringLength(100)]
        public string? InsuranceProvider { get; set; }

        [Display(Name = "Primary Care Physician")]
        [StringLength(100)]
        public string? PrimaryCarePhysician { get; set; }

        [Display(Name = "Allergies")]
        [StringLength(500)]
        public string? Allergies { get; set; }

        [Display(Name = "Medical History")]
        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [Display(Name = "Current Medications")]
        [StringLength(500)]
        public string? CurrentMedications { get; set; }
    }
} 