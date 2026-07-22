using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("centers")]
    public class Center
    {
        [Key]
        [StringLength(5)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string CenterName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Province { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        [Required]
        public string SubDistrict { get; set; } = string.Empty;

        [StringLength(5)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Relationship
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();

        public ICollection<SosRequest> SosRequests { get; set; }
        = new List<SosRequest>();
        public ICollection<CenterInventory> Inventories { get; set; }
    = new List<CenterInventory>();
    }
}