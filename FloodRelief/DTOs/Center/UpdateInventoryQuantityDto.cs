using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Center
{
    public class UpdateInventoryQuantityDto
    {
        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ReliefItemId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(30)]
        public string TransactionType { get; set; } = string.Empty;

        [StringLength(30)]
        public string? ReferenceType { get; set; }

        [StringLength(10)]
        public string? ReferenceId { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }
    }
}