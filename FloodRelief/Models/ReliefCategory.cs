using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("relief_categories")]
    public class ReliefCategory
    {
        [Key]
        [StringLength(2)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ReliefItem> ReliefItems { get; set; } = new List<ReliefItem>();
    }
}