using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("inventory_transactions")]
    public class InventoryTransaction
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CenterInventoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string TransactionType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int BalanceAfter { get; set; }

        [StringLength(30)]
        public string? ReferenceType { get; set; }

        [StringLength(10)]
        public string? ReferenceId { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        [StringLength(5)]
        public string? StaffId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public CenterInventory CenterInventory { get; set; } = null!;

        public Staff? Staff { get; set; }
    }
}