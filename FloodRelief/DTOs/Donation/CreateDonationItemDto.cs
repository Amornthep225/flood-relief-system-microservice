using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Donation
{
    public class CreateDonationItemDto
    {
        [Required]
        public string ReliefItemId { get; set; }
            = string.Empty;


        [Required]
        public int Quantity { get; set; }
    }
}