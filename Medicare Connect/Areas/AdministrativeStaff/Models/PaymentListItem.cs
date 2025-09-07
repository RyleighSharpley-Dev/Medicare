namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class PaymentListItem
    {
        public string Id { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? StripeSessionId { get; set; }
        public string? ReferenceId { get; set; }
        public string? Notes { get; set; }
    }
} 