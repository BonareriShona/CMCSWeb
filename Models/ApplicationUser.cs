using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCSWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Department")]
        [StringLength(100)]
        public string? Department { get; set; }

        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal HourlyRate { get; set; }

        [NotMapped]
        [Display(Name = "Role")]
        public string UserRole { get; set; } = "Lecturer";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for claims
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public ApplicationUser()
        {
            Claims = new List<Claim>();
        }
    }
}