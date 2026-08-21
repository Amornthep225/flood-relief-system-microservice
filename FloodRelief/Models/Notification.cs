using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [StringLength(10)]
        public string? UserId { get; set; }

        [StringLength(5)]
        public string? StaffId { get; set; }

        [Required]
        [StringLength(40)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [StringLength(30)]
        public string? ReferenceType { get; set; }

        [StringLength(10)]
        public string? ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff? Staff { get; set; }
    }
}
