using System;

namespace EventOrganizer_ASP.NET.ViewModels
{
    public class AdminReviewVM
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
