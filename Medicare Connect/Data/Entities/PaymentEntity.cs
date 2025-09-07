using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Data.Entities
{
    public enum PaymentType
    {
        Appointment,
        PrescriptionRefill,
        Consultation,
        Other
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }

    public class PaymentEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        public PaymentType PaymentType { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [StringLength(100)]
        public string? StripeSessionId { get; set; }

        [StringLength(100)]
        public string? StripePaymentIntentId { get; set; }

        [StringLength(100)]
        public string? ReferenceId { get; set; } // Appointment ID, Prescription ID, etc.

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for patient information
        public virtual IdentityUser? Patient { get; set; }
    }
} 