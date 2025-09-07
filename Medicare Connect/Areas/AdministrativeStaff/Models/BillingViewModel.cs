using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class BillingViewModel
    {
        [Required]
        [Display(Name = "Patient")]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Amount")]
        [Range(0.01, 10000.00)]
        public decimal Amount { get; set; }

        [Display(Name = "Insurance Coverage")]
        public bool InsuranceCoverage { get; set; } = false;

        [Display(Name = "Insurance Amount")]
        [Range(0.00, 10000.00)]
        public decimal? InsuranceAmount { get; set; }

        [Display(Name = "Patient Responsibility")]
        [Range(0.00, 10000.00)]
        public decimal PatientResponsibility { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class BillingListItem
    {
        public string Id { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }
} 