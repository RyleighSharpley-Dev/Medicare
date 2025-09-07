using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class RecordStore
{
	private static readonly Dictionary<string, List<PatientRecord>> PatientRecords = new();

	public static string GetPatientKey(ClaimsPrincipal user)
	{
		var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!string.IsNullOrWhiteSpace(id)) return id;
		var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? "unknown";
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	public static List<PatientRecord> GetList(string key)
	{
		if (!PatientRecords.ContainsKey(key))
		{
			PatientRecords[key] = new List<PatientRecord>();
		}
		return PatientRecords[key];
	}

	public static void Add(string key, PatientRecord record)
	{
		GetList(key).Add(record);
	}

	public static PatientRecord? Find(string key, Guid id)
	{
		return GetList(key).FirstOrDefault(r => r.Id == id);
	}

	public static bool Remove(string key, Guid id)
	{
		var list = GetList(key);
		var r = list.FirstOrDefault(x => x.Id == id);
		if (r == null) return false;
		return list.Remove(r);
	}
} 