using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("geographies")]
    public class ThaiGeography
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        public ICollection<ThaiProvince> Provinces { get; set; }
            = new List<ThaiProvince>();
    }
}
