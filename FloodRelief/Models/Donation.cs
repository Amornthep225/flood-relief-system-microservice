using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("donations")]
    public class Donation
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;


        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;


        [Required]
        public string DonationType { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public int Quantity { get; set; }

        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;


        public string? ImageUrl { get; set; }

        public string? QRCode { get; set; }
        public string Status { get; set; } = "Pending";


        public DateTime CreatedAt { get; set; }
            = DateTime.Now;


        public DateTime? UpdatedAt { get; set; }



        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        [ForeignKey(nameof(CenterId))]
        public Center? Center { get; set; }
        public ICollection<DonationItem> Items { get; set; }
    = new List<DonationItem>();
    }
}