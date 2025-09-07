using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medicare_Connect.Areas.Patients.Models
{
    public class PatientProfileViewModel
    {
        // Profile Photo
        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }
        
        public string? ProfilePhotoPath { get; set; }
        
        // Personal Information
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Display(Name = "Gender")]
        public string Gender { get; set; } = string.Empty;

        // Contact Information
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        // Emergency Contact
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Display(Name = "Emergency Contact Relationship")]
        public string EmergencyContactRelationship { get; set; } = string.Empty;

        [Display(Name = "Emergency Contact Phone")]
        [Phone]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Medical Information
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [Display(Name = "Allergies")]
        public string Allergies { get; set; } = string.Empty;

        [Display(Name = "Chronic Conditions")]
        public string ChronicConditions { get; set; } = string.Empty;

        [Display(Name = "Current Medications")]
        public string CurrentMedications { get; set; } = string.Empty;

        // Insurance Information
        [Display(Name = "Medical Aid Scheme")]
        public string MedicalAidScheme { get; set; } = string.Empty;

        [Display(Name = "Medical Aid Number")]
        public string MedicalAidNumber { get; set; } = string.Empty;

        [Display(Name = "Main Member Name")]
        public string MainMemberName { get; set; } = string.Empty;

        [Display(Name = "Dependant Code")]
        public string DependantCode { get; set; } = string.Empty;
    }
}
