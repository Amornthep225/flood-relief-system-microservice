using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("donation_allocations")]
    public class DonationAllocation
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DonationBatchId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string SosRequestId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime AllocatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(DonationBatchId))]
        public DonationBatch DonationBatch { get; set; } = null!;

        [ForeignKey(nameof(SosRequestId))]
        public SosRequest SosRequest { get; set; } = null!;

        [ForeignKey(nameof(ReliefItemId))]
        public ReliefItem ReliefItem { get; set; } = null!;
    }
}
