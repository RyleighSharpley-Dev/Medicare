using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.Patients.Models
{
	public class SettingsViewModel
	{
		[Display(Name = "Receive email notifications")]
		public bool ReceiveEmailNotifications { get; set; } = true;

		[Display(Name = "Receive reminder emails for appointments")]
		public bool ReceiveReminderEmails { get; set; } = true;

		[Display(Name = "Default appointment location")]
		[StringLength(128)]
		public string? DefaultLocation { get; set; } = "Main Clinic";

		[Display(Name = "Preferred language")]
		[StringLength(32)]
		public string PreferredLanguage { get; set; } = "English";

		[Display(Name = "Time zone")]
		[StringLength(64)]
		public string TimeZone { get; set; } = "Africa/Johannesburg";

		[Display(Name = "Currency")]
		[StringLength(8)]
		public string Currency { get; set; } = "ZAR";

		[Display(Name = "Allow marketing emails")]
		public bool AllowMarketing { get; set; } = false;
	}
} 