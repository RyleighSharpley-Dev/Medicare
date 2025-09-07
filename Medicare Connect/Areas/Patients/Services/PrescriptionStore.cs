using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class PrescriptionStore
{
	private static readonly Dictionary<string, List<Prescription>> PatientPrescriptions = new();
	private static readonly List<RefillRequest> RefillRequests = new();

	public static string GetPatientKey(ClaimsPrincipal user)
	{
		var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!string.IsNullOrWhiteSpace(id)) return id;
		var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? "unknown";
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	public static List<Prescription> GetPrescriptions(string patientKey)
	{
		if (!PatientPrescriptions.ContainsKey(patientKey))
		{
			PatientPrescriptions[patientKey] = new List<Prescription>();
		}
		return PatientPrescriptions[patientKey];
	}

	public static void AddPrescription(string patientKey, Prescription p)
	{
		GetPrescriptions(patientKey).Add(p);
	}

	public static Prescription? FindPrescription(Guid id)
	{
		return PatientPrescriptions.Values.SelectMany(x => x).FirstOrDefault(p => p.Id == id);
	}

	public static IEnumerable<RefillRequest> GetRefillRequestsForDoctor()
	{
		return RefillRequests.Where(r => r.Status == RefillRequestStatus.Pending).OrderBy(r => r.RequestedAt);
	}

	public static IEnumerable<RefillRequest> GetRefillRequestsForPatient(string key)
	{
		return RefillRequests.Where(r => r.PatientKey == key).OrderByDescending(r => r.RequestedAt);
	}

	public static void AddRefillRequest(RefillRequest r)
	{
		RefillRequests.Add(r);
	}

	public static void ApproveRefill(RefillRequest r, string doctorName)
	{
		r.Status = RefillRequestStatus.Approved;
		r.DecisionBy = doctorName;
		r.DecisionAt = DateTime.UtcNow;
		var p = FindPrescription(r.PrescriptionId);
		if (p != null && p.RefillsUsed < p.RefillsTotal) p.RefillsUsed++;
	}

	public static void RejectRefill(RefillRequest r, string doctorName, string? notes)
	{
		r.Status = RefillRequestStatus.Rejected;
		r.DecisionBy = doctorName;
		r.DecisionAt = DateTime.UtcNow;
		r.DecisionNotes = notes;
	}
} 