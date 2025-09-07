using System;
using System.Collections.Generic;
using Medicare_Connect.Areas.Patients.Models;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.Patients.Models
{
	public class DashboardViewModel
	{
		public int UpcomingAppointmentsCount { get; set; }
		public DateTime? NextAppointmentUtc { get; set; }

		public int ActivePrescriptionsCount { get; set; }
		public int TotalAvailableRefills { get; set; }

		public long? LastPaymentCents { get; set; }
		public DateTime? LastPaymentAtUtc { get; set; }

		public int RemindersCount { get; set; }
		public DateTime? NextReminderUtc { get; set; }

		public List<PatientRecord> RecentRecords { get; set; } = new();
		public List<PaymentEntity> RecentPayments { get; set; } = new();
	}
} 