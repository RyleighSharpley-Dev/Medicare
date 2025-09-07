using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
	public enum ReminderType
	{
		Appointment,
		Medication,
		Custom
	}

	public class Reminder
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string PatientKey { get; set; } = string.Empty;
		[Required]
		[StringLength(100)]
		public string Title { get; set; } = string.Empty;
		public ReminderType Type { get; set; } = ReminderType.Custom;
		[Display(Name = "Due Date & Time")]
		public DateTime? DueAt { get; set; }
		[StringLength(256)]
		public string? Notes { get; set; }
		public bool IsCompleted { get; set; }
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

		// Link to related appointment when applicable
		public Guid? RelatedAppointmentId { get; set; }
	}

	public class ReminderCreateModel
	{
		[Required]
		[StringLength(100)]
		public string Title { get; set; } = string.Empty;
		[Required]
		public ReminderType Type { get; set; } = ReminderType.Custom;
		[Display(Name = "Due Date & Time")]
		public DateTime? DueAt { get; set; }
		[StringLength(256)]
		public string? Notes { get; set; }
	}
} 