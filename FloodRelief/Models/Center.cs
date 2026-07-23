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
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // ใช้ nullable ชั่วคราวเพื่อรองรับข้อมูลศูนย์เดิมที่ยังไม่มี Address ID
        public int? ProvinceId { get; set; }

        public int? DistrictId { get; set; }

        public int? SubDistrictId { get; set; }

        // เก็บชื่อไว้เพื่อรองรับ Response และข้อมูลเดิมของระบบ
        [Required]
        [StringLength(150)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string District { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string SubDistrict { get; set; } = string.Empty;

        [StringLength(5)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(10)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(150)]
        public string ContactName { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ProvinceId))]
        public ThaiProvince? ProvinceData { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public ThaiDistrict? DistrictData { get; set; }

        [ForeignKey(nameof(SubDistrictId))]
        public ThaiSubDistrict? SubDistrictData { get; set; }

        public ICollection<Staff> Staffs { get; set; }
            = new List<Staff>();

        public ICollection<SosRequest> SosRequests { get; set; }
            = new List<SosRequest>();

        public ICollection<CenterInventory> Inventories { get; set; }
            = new List<CenterInventory>();
    }
}
