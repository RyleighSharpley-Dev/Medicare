using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Nurses.Models
{
    public class PatientCareViewModel
    {
        public string PatientKey { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        
        [Display(Name = "Vital Signs")]
        public string? VitalSigns { get; set; }
        
        [Display(Name = "Temperature (°C)")]
        [Range(35.0, 42.0)]
        public decimal? Temperature { get; set; }
        
        [Display(Name = "Blood Pressure (mmHg)")]
        public string? BloodPressure { get; set; }
        
        [Display(Name = "Heart Rate (bpm)")]
        [Range(40, 200)]
        public int? HeartRate { get; set; }
        
        [Display(Name = "Weight (kg)")]
        [Range(0.5, 300.0)]
        public decimal? Weight { get; set; }
        
        [Display(Name = "Height (cm)")]
        [Range(30, 250)]
        public int? Height { get; set; }
        
        [Display(Name = "Nursing Notes")]
        [StringLength(1000)]
        public string? NursingNotes { get; set; }
        
        [Display(Name = "Care Plan")]
        [StringLength(1000)]
        public string? CarePlan { get; set; }
        
        [Display(Name = "Medications Given")]
        [StringLength(500)]
        public string? MedicationsGiven { get; set; }
        
        [Display(Name = "Next Follow-up")]
        public DateTime? NextFollowUp { get; set; }
    }
} 