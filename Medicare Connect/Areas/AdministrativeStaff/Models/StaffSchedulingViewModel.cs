using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class StaffSchedulingViewModel
    {
        [Required]
        [Display(Name = "Staff Member")]
        public string StaffId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Shift Type")]
        public string ShiftType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class LeaveRequestViewModel
    {
        [Required]
        [Display(Name = "Staff Member")]
        public string StaffId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(8);

        [Required]
        [Display(Name = "Reason")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Emergency Contact")]
        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }

    public class StaffScheduleListItem
    {
        public string Id { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ShiftType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 