using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Center
{
    public class CreateCenterRequestDto
    {
        [Required]
        public string CenterName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string Province { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        [Required]
        public string SubDistrict { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}