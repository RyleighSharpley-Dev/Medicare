using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
	public enum ConsultationStatus
	{
		Planned,
		Completed
	}

	public class Consultation
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string PatientKey { get; set; } = string.Empty;
		public string PatientName { get; set; } = string.Empty;
		public string DoctorName { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string Subject { get; set; } = string.Empty;
		[Display(Name = "Date & Time")]
		public DateTime WhenUtc { get; set; } = DateTime.UtcNow;
		[Range(0, 480)]
		public int DurationMinutes { get; set; } = 30;
		[StringLength(1000)]
		public string? Notes { get; set; }
		public ConsultationStatus Status { get; set; } = ConsultationStatus.Planned;
	}

	public class ConsultationCreateModel
	{
		[Required, StringLength(128)]
		public string Subject { get; set; } = string.Empty;
		[Display(Name = "Date & Time")]
		public DateTime? WhenUtc { get; set; }
		[Range(0, 480)]
		public int DurationMinutes { get; set; } = 30;
		[StringLength(1000)]
		public string? Notes { get; set; }
	}
} 