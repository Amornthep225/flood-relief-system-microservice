using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("sub_districts")]
    public class ThaiSubDistrict
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("zip_code")]
        public int ZipCode { get; set; }

        [Required]
        [StringLength(150)]
        [Column("name_th")]
        public string NameTh { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Column("district_id")]
        public int DistrictId { get; set; }

        [Column("lat")]
        public double? Latitude { get; set; }

        [Column("long")]
        public double? Longitude { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public ThaiDistrict District { get; set; } = null!;

        public ICollection<Center> Centers { get; set; }
            = new List<Center>();
    }
}
