using Microsoft.AspNetCore.Identity;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class DemoDataSeeder
{
	public static async Task SeedAsync(IServiceProvider provider)
	{
		var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
		var patients = await userManager.GetUsersInRoleAsync("Patients");

		foreach (var user in patients)
		{
			var key = user.Id;
			var list = RecordStore.GetList(key);
			if (list.Count > 0)
			{
				continue; // already has records
			}

			var now = DateTime.UtcNow;
			RecordStore.Add(key, new PatientRecord
			{
				PatientKey = key,
				Title = "Blood Panel",
				Type = RecordType.LabResult,
				Date = now.AddDays(-20),
				Notes = "Routine blood work; monitor LDL.",
				FileName = null,
				ContentType = null,
				FileSizeBytes = null,
				RelativePath = null
			});
			RecordStore.Add(key, new PatientRecord
			{
				PatientKey = key,
				Title = "X-Ray Chest",
				Type = RecordType.Imaging,
				Date = now.AddDays(-45),
				Notes = "No acute findings.",
				FileName = null,
				ContentType = null,
				FileSizeBytes = null,
				RelativePath = null
			});
			RecordStore.Add(key, new PatientRecord
			{
				PatientKey = key,
				Title = "Amoxicillin 500mg",
				Type = RecordType.Prescription,
				Date = now.AddDays(-7),
				Notes = "TID for 7 days.",
				FileName = null,
				ContentType = null,
				FileSizeBytes = null,
				RelativePath = null
			});
		}
	}
} 