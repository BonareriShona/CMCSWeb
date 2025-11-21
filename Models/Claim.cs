using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCSWeb.Models
{
    public enum ClaimStatus
    {
        Pending,
        Verified,
        Approved,
        Rejected
    }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(0.1, 180, ErrorMessage = "Hours Worked cannot exceed 180 hours per month.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0.1, 10000, ErrorMessage = "Hourly Rate must be greater than 0.")]
        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        // FIXED: Use computed property that doesn't get mapped to database
        [Display(Name = "Total Amount")]
        [NotMapped] // This tells EF Core to ignore this property for database mapping
        public decimal TotalAmount => HoursWorked * HourlyRate;

        // Add this property to store the value in database if needed
        [Display(Name = "Stored Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StoredTotalAmount { get; set; }

        [Display(Name = "Documentation")]
        [MaxLength(500, ErrorMessage = "Documentation cannot exceed 500 characters.")]
        public string Documentation { get; set; } = string.Empty;

        [Display(Name = "Uploaded Document Path")]
        public string? DocumentPath { get; set; }

        [Display(Name = "Status")]
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // For tracking approval workflow
        public DateTime? VerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public string? ApprovedBy { get; set; }

        // Month and Year for reporting
        public int ClaimMonth { get; set; } = DateTime.Now.Month;
        public int ClaimYear { get; set; } = DateTime.Now.Year;

        // Method to update stored total amount
        public void UpdateStoredTotalAmount()
        {
            StoredTotalAmount = HoursWorked * HourlyRate;
        }

        // Constructor to initialize stored amount
        public Claim()
        {
            UpdateStoredTotalAmount();
        }
    }
}