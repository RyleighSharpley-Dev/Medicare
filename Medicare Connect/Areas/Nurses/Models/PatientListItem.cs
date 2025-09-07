namespace Medicare_Connect.Areas.Nurses.Models
{
    public class PatientListItem
    {
        public string PatientKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastVisit { get; set; }
        public string Status { get; set; } = "Active";
    }
} 