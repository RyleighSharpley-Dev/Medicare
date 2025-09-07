using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Data.Entities
{
	public class ConsultationEntity
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		public string PatientKey { get; set; } = string.Empty;
		[Required]
		public string PatientName { get; set; } = string.Empty;
		[Required]
		public string DoctorName { get; set; } = string.Empty;
		[Required, StringLength(128)]
		public string Subject { get; set; } = string.Empty;
		public DateTime WhenUtc { get; set; }
		[Range(0, 480)]
		public int DurationMinutes { get; set; }
		[StringLength(1000)]
		public string? Notes { get; set; }
		public int Status { get; set; }
	}
} 