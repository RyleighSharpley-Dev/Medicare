using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class AppointmentStore
{
	// In-memory demo store; replace with persistence later
	private static readonly Dictionary<string, List<Appointment>> PatientAppointments = new();

	// Demo pricing map (ZAR cents)
	private static readonly Dictionary<string, long> AppointmentTypeToPriceCents = new(StringComparer.OrdinalIgnoreCase)
	{
		["General Checkup"] = 50000,
		["Follow-up"] = 30000,
		["Lab Results"] = 20000,
		["Vaccination"] = 40000,
	};

	public static long GetPriceCentsFor(string appointmentType)
	{
		if (string.IsNullOrWhiteSpace(appointmentType)) return 50000;
		return AppointmentTypeToPriceCents.TryGetValue(appointmentType, out var cents) ? cents : 50000;
	}

	public static string GetPatientKey(ClaimsPrincipal user)
	{
		var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!string.IsNullOrWhiteSpace(id)) return id;
		var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? "unknown";
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	public static List<Appointment> GetList(string patientKey)
	{
		if (!PatientAppointments.ContainsKey(patientKey))
		{
			PatientAppointments[patientKey] = new List<Appointment>();
		}
		return PatientAppointments[patientKey];
	}

	public static void Add(string patientKey, Appointment appointment)
	{
		var list = GetList(patientKey);
		list.Add(appointment);
	}

	public static Appointment? FindById(string patientKey, Guid id)
	{
		var list = GetList(patientKey);
		return list.FirstOrDefault(a => a.Id == id);
	}

	public static IReadOnlyDictionary<string, List<Appointment>> GetAllByPatientKey()
	{
		// Return a read-only snapshot reference (safe for read in current single-process demo)
		return PatientAppointments;
	}
} 