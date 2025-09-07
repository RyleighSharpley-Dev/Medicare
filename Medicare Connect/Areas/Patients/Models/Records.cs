using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
	public enum RecordType
	{
		Report,
		Prescription,
		LabResult,
		Imaging,
		Other
	}

	public class PatientRecord
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string PatientKey { get; set; } = string.Empty;
		[Required, StringLength(100)]
		public string Title { get; set; } = string.Empty;
		public RecordType Type { get; set; } = RecordType.Other;
		public DateTime Date { get; set; } = DateTime.UtcNow;
		[StringLength(256)]
		public string? Notes { get; set; }

		// Optional file metadata
		public string? FileName { get; set; }
		public string? ContentType { get; set; }
		public long? FileSizeBytes { get; set; }
		public string? RelativePath { get; set; } // e.g. /uploads/records/...
	}

	public class RecordCreateModel
	{
		[Required, StringLength(100)]
		public string Title { get; set; } = string.Empty;
		[Required]
		public RecordType Type { get; set; } = RecordType.Other;
		[Display(Name = "Date")]
		public DateTime? Date { get; set; }
		[StringLength(256)]
		public string? Notes { get; set; }
	}
} 