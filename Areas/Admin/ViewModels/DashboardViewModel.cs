using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalRegistrations { get; set; }
        public int TotalUsers { get; set; }

        public List<RegistrationVM> RecentRegistrations { get; set; } = new();
    }
}