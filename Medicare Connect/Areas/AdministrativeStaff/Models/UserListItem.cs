using Microsoft.AspNetCore.Identity;

namespace Medicare_Connect.Areas.AdministrativeStaff.Models
{
    public class UserListItem
    {
        public IdentityUser User { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
    }
} 