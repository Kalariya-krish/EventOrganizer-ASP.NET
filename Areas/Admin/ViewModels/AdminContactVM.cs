using System;

namespace EventOrganizer_ASP.NET.Areas.Admin.ViewModels
{
    public class AdminContactVM
    {
        public int MessageId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}