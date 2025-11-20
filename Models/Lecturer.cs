
using System.ComponentModel.DataAnnotations;

namespace CMCSWeb.Models
{
    public class Lecturer
    {
        [Key]
        public int LecturerId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Computed property for full name
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation property
        public virtual ICollection<Claim>? Claims { get; set; }

        public decimal HourlyRate { get; set; } // Added HourlyRate
    }
}