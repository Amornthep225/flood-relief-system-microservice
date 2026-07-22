using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Donation
{
    public class UpdateDonationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}