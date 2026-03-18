using System;
using System.ComponentModel.DataAnnotations;

namespace EventOrganizer_ASP.NET.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters")]
        public string Password { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        //public DateTime CreatedAt { get; set; }
    }
}