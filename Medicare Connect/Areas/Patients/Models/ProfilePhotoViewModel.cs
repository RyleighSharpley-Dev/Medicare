using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medicare_Connect.Areas.Patients.Models
{
    public class ProfilePhotoViewModel
    {
        [Display(Name = "Profile Photo")]
        public IFormFile? PhotoFile { get; set; }
        
        public string? ExistingPhotoPath { get; set; }
    }
}
