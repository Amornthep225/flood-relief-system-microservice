using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("provinces")]
    public class ThaiProvince
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Column("name_th")]
        public string NameTh { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Column("geography_id")]
        public int GeographyId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(GeographyId))]
        public ThaiGeography Geography { get; set; } = null!;

        public ICollection<ThaiDistrict> Districts { get; set; }
            = new List<ThaiDistrict>();

        public ICollection<Center> Centers { get; set; }
            = new List<Center>();
    }
}
