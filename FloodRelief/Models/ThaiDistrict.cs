using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("districts")]
    public class ThaiDistrict
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

        [Column("province_id")]
        public int ProvinceId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(ProvinceId))]
        public ThaiProvince Province { get; set; } = null!;

        public ICollection<ThaiSubDistrict> SubDistricts { get; set; }
            = new List<ThaiSubDistrict>();

        public ICollection<Center> Centers { get; set; }
            = new List<Center>();
    }
}
