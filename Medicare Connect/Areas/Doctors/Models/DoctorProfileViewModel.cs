using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medicare_Connect.Areas.Doctors.Models
{
	public class DoctorProfileViewModel
	{
		[Display(Name = "Full name")]
		public string FullName { get; set; } = string.Empty;

		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Phone]
		[Display(Name = "Phone number")]
		public string? PhoneNumber { get; set; }

		[Display(Name = "Department")]
		public string? Department { get; set; }

		[Display(Name = "Specialty")]
		public string? Specialty { get; set; }

		[Display(Name = "Office location")]
		public string? OfficeLocation { get; set; }

		[StringLength(1000)]
		[Display(Name = "Bio / About")]
		public string? Bio { get; set; }

		public string? ProfilePhotoPath { get; set; }

		[Display(Name = "Profile photo")]
		public IFormFile? ProfilePhoto { get; set; }
	}
} 