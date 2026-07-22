using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("sos_request_items")]
    public class SosRequestItem
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string SosRequestId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(SosRequestId))]
        public SosRequest? SosRequest { get; set; }

        [ForeignKey(nameof(ReliefItemId))]
        public ReliefItem? ReliefItem { get; set; }
    }
}