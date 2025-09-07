using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int UpcomingAppointments { get; set; }
        public decimal PaymentsTodayAmount { get; set; }
        public int OutstandingBills { get; set; }

        public List<AppointmentEntity> NextAppointments { get; set; } = new();
        public List<PaymentEntity> RecentPayments { get; set; } = new();
    }
} 