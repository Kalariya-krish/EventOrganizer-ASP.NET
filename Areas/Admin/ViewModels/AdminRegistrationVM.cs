using System;

namespace EventOrganizer_ASP.NET.Areas.Admin.ViewModels
{
    public class AdminRegistrationVM
    {
        public int RegistrationId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}