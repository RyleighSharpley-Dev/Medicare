using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
	public enum PrescriptionStatus
	{
		Active,
		Completed,
		Cancelled
	}

	public enum RefillRequestStatus
	{
		Pending,
		Approved,
		Rejected
	}

	public class Prescription
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string PatientKey { get; set; } = string.Empty;
		public string PatientName { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string Medication { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string Dosage { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string PrescribedBy { get; set; } = string.Empty;
		public DateTime Date { get; set; } = DateTime.UtcNow;
		[StringLength(256)]
		public string? Notes { get; set; }
		public int RefillsTotal { get; set; } = 0;
		public int RefillsUsed { get; set; } = 0;
		public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
	}

	public class RefillRequest
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid PrescriptionId { get; set; }
		public string PatientKey { get; set; } = string.Empty;
		public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
		public RefillRequestStatus Status { get; set; } = RefillRequestStatus.Pending;
		public string? DecisionBy { get; set; }
		public DateTime? DecisionAt { get; set; }
		public string? DecisionNotes { get; set; }
		public long PriceCents { get; set; } = 8000; // default refill fee (R80.00)
		public bool IsPaid { get; set; } = false;
	}

	public class PrescriptionCreateModel
	{
		[Required, StringLength(128)]
		public string Medication { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string Dosage { get; set; } = string.Empty;
		[Range(0, 10)]
		public int RefillsTotal { get; set; } = 0;
		[StringLength(256)]
		public string? Notes { get; set; }
	}
} 