using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Center
{
    public class UpdateCenterRequestDto
    {
        [Required]
        [StringLength(150)]
        public string CenterName { get; set; }
            = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; }
            = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int ProvinceId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DistrictId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SubDistrictId { get; set; }

        [StringLength(10)]
        public string PhoneNumber { get; set; }
            = string.Empty;

        [StringLength(150)]
        public string ContactName { get; set; }
            = string.Empty;

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        public bool IsActive { get; set; }
    }
}