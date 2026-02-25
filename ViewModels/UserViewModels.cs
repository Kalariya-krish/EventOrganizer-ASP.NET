using System;
using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.ViewModels
{
    public class EventCardVM
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int RegisteredCount { get; set; }
    }

    public class EventDetailVM
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
        public int RegisteredCount { get; set; }
        public bool IsRegistered { get; set; }
        public List<EventReviewVM> Reviews { get; set; } = new();
    }

    public class EventReviewVM
    {
        public string FullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MyEventVM
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasReview { get; set; }
    }

    public class UserProfileVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}