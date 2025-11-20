using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CMCSWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        // Personal Information
        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        // Hourly rate for lecturers (HR sets this)
        [Range(0.1, 10000, ErrorMessage = "Hourly Rate must be greater than 0.")]
        public decimal? HourlyRate { get; set; }

        // Role specification
        public string UserRole { get; set; } = "Lecturer";

        // Account status
        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}