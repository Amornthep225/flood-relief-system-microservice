using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("donation_batches")]
    public class DonationBatch
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DonationId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DonationItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int RemainingQuantity { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(DonationId))]
        public Donation Donation { get; set; } = null!;

        [ForeignKey(nameof(DonationItemId))]
        public DonationItem DonationItem { get; set; } = null!;

        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; } = null!;

        [ForeignKey(nameof(ReliefItemId))]
        public ReliefItem ReliefItem { get; set; } = null!;

        public ICollection<DonationAllocation> Allocations { get; set; }
            = new List<DonationAllocation>();
    }
}
