namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class PatientListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
} 