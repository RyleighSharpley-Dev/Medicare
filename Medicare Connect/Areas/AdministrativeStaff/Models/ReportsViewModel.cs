using System.ComponentModel.DataAnnotations;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class ReportRequestViewModel
    {
        [Required]
        [Display(Name = "Report Type")]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Doctor")]
        public string? DoctorId { get; set; }

        [Display(Name = "Patient")]
        public string? PatientId { get; set; }

        [Display(Name = "Format")]
        public string Format { get; set; } = "PDF";

        [Display(Name = "Include Charts")]
        public bool IncludeCharts { get; set; } = true;

        [Display(Name = "Include Details")]
        public bool IncludeDetails { get; set; } = true;
    }

    public class ReportSummaryViewModel
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRecords { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public string Department { get; set; } = string.Empty;
        public List<ChartDataPoint> ChartData { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DashboardMetricsViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalNurses { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public int OverdueBills { get; set; }
        public List<RecentActivity> RecentActivities { get; set; } = new();
    }

    public class RecentActivity
    {
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }
} 