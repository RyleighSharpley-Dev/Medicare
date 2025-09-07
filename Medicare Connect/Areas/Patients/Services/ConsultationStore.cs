using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class ConsultationStore
{
	private static readonly Dictionary<string, List<Consultation>> PatientConsultations = new();

	public static List<Consultation> GetForPatient(string patientKey)
	{
		if (!PatientConsultations.ContainsKey(patientKey))
		{
			PatientConsultations[patientKey] = new List<Consultation>();
		}
		return PatientConsultations[patientKey];
	}

	public static void Add(string patientKey, Consultation c)
	{
		GetForPatient(patientKey).Add(c);
	}
} 