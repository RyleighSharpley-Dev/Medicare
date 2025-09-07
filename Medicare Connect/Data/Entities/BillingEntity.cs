using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Data.Entities
{
    public class BillingEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? InsuranceAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PatientResponsibility { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsOverdue => DueDate < DateTime.Today;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for patient information
        public virtual IdentityUser? Patient { get; set; }
    }
} 