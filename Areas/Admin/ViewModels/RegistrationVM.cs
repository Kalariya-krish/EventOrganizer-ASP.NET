using System;

namespace EventOrganizer_ASP.NET.Areas.Admin.ViewModels
{
    public class RegistrationVM
    {
        public string UserName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}