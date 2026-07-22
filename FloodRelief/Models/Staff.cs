using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("staffs")]
    public class Staff
    {
        [Key]
        [StringLength(5)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Staff";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Relationships
        [ForeignKey(nameof(CenterId))]
        public Center? Center { get; set; }

        public ICollection<SosRequest> AssignedSosRequests { get; set; }
            = new List<SosRequest>();

        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
            = new List<InventoryTransaction>();
    }
}