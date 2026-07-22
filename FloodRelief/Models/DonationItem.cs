using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("donation_items")]
    public class DonationItem
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;


        [Required]
        [StringLength(10)]
        public string DonationId { get; set; } = string.Empty;


        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;


        [Required]
        public int Quantity { get; set; }



        public string Unit { get; set; } = string.Empty;



        [ForeignKey(nameof(DonationId))]
        public Donation? Donation { get; set; }



        [ForeignKey(nameof(ReliefItemId))]
        public ReliefItem? ReliefItem { get; set; }
    }
}