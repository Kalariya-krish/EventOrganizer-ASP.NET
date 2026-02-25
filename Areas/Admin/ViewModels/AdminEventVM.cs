using System;
using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.Areas.Admin.ViewModels
{
    public class AdminEventVM
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AddEventVM
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<CategoryVM> Categories { get; set; } = new();
    }

    public class EditEventVM
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}