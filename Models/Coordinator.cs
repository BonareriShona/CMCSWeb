using System.ComponentModel.DataAnnotations;

namespace CMCSWeb.Models
{
    public class Coordinator
    {
        [Key]
        public int CoordinatorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}