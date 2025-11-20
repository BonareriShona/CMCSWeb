using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCSWeb.Models
{
    public enum ClaimStatus
    {
        Pending,     // Submitted by Lecturer, awaiting coordinator
        Verified,    // Approved by Coordinator, awaiting Manager
        Approved,    // Approved by Manager
        Rejected     // Rejected by Coordinator or Manager
    }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        // Link to ApplicationUser instead of LecturerName
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(0.1, 180, ErrorMessage = "Hours Worked cannot exceed 180 hours per month.")]
        [Display(Name = "Hours Worked")]
        public double HoursWorked { get; set; }

        // Auto-populated from user's hourly rate
        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0.1, 10000, ErrorMessage = "Hourly Rate must be greater than 0.")]
        [Display(Name = "Hourly Rate")]
        public double HourlyRate { get; set; }

        // Auto-calculated total
        [Display(Name = "Total Amount")]
        public double TotalAmount => HoursWorked * HourlyRate;

        [Display(Name = "Notes")]
        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string Notes { get; set; } = string.Empty;

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
    }
}