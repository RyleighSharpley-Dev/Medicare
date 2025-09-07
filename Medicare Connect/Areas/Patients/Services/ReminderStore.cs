using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Services;

public static class ReminderStore
{
	private static readonly Dictionary<string, List<Reminder>> PatientReminders = new();

	public static string GetPatientKey(ClaimsPrincipal user)
	{
		var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!string.IsNullOrWhiteSpace(id)) return id;
		var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name ?? "unknown";
		return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email)).TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}

	public static List<Reminder> GetList(string key)
	{
		if (!PatientReminders.ContainsKey(key))
		{
			PatientReminders[key] = new List<Reminder>();
		}
		return PatientReminders[key];
	}

	public static void Add(string key, Reminder reminder)
	{
		GetList(key).Add(reminder);
	}

	public static Reminder? Find(string key, Guid id)
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

	public static void AddAppointmentReminders(string key, Guid appointmentId, string appointmentType, DateTime appointmentDate)
	{
		var schedule = new[] { TimeSpan.FromHours(24), TimeSpan.FromHours(2) };
		foreach (var before in schedule)
		{
			var due = appointmentDate - before;
			Add(key, new Reminder
			{
				PatientKey = key,
				Title = $"Upcoming {appointmentType} appointment",
				Type = ReminderType.Appointment,
				DueAt = due,
				Notes = $"Your appointment is on {appointmentDate:ddd, dd MMM yyyy HH:mm}",
				RelatedAppointmentId = appointmentId
			});
		}
	}

	public static void RemoveAppointmentReminders(string key, Guid appointmentId)
	{
		var list = GetList(key);
		list.RemoveAll(r => r.Type == ReminderType.Appointment && r.RelatedAppointmentId == appointmentId);
	}
} 