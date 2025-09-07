namespace Medicare_Connect.Areas.Patients.Models
{
    public class PatientBillingViewModel
    {
        public List<PatientBillingItem> BillingRecords { get; set; } = new();
        public List<PatientPaymentItem> PaymentRecords { get; set; } = new();
        public BillingSummary Summary { get; set; } = new();
    }

    public class PatientBillingItem
    {
        public string Id { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PatientPaymentItem
    {
        public string Id { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class BillingSummary
    {
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int BillingCount { get; set; }
        public int PaymentCount { get; set; }
    }

    public class PatientBillingDetailViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OutstandingBillsViewModel
    {
        public List<OutstandingBillItem> Bills { get; set; } = new();
    }

    public class OutstandingBillItem
    {
        public string Id { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal PatientResponsibility { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentHistoryViewModel
    {
        public List<PaymentHistoryItem> Payments { get; set; } = new();
    }

    public class PaymentHistoryItem
    {
        public string Id { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
        public string? ReferenceId { get; set; }
    }
} 