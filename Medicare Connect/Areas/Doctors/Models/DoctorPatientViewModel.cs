namespace Medicare_Connect.Areas.Doctors.Models
{
    public class DoctorPatientViewModel
    {
        public string PatientKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public DateTime? NextAppointment { get; set; }
        public DateTime? LastAppointment { get; set; }
    }
}


