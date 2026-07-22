using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("center_inventories")]
    public class CenterInventory
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public Center Center { get; set; } = null!;

        public ReliefItem ReliefItem { get; set; } = null!;

        public ICollection<InventoryTransaction> Transactions { get; set; }
            = new List<InventoryTransaction>();
    }
}