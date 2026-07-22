using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("relief_items")]
    public class ReliefItem
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string ReliefCategoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ReliefCategoryId")]
        public ReliefCategory? ReliefCategory { get; set; }

        public ICollection<SosRequestItem> SosRequestItems { get; set; } = new List<SosRequestItem>();
    }
}