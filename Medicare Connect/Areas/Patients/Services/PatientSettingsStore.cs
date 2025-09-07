using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class PatientSettingsStore
{
	private static readonly Dictionary<string, SettingsViewModel> SettingsByPatient = new();

	public static string GetPatientKey(ClaimsPrincipal user)
	{
		return AppointmentStore.GetPatientKey(user);
	}

	public static SettingsViewModel Get(string patientKey)
	{
		if (!SettingsByPatient.TryGetValue(patientKey, out var settings))
		{
			settings = new SettingsViewModel();
			SettingsByPatient[patientKey] = settings;
		}
		return settings;
	}

	public static void Save(string patientKey, SettingsViewModel model)
	{
		SettingsByPatient[patientKey] = model;
	}
} 