using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Donation
{
    public class CreateDonationDto
    {
        [Required]
        public string CenterId { get; set; } = string.Empty;


        [Required]
        [MinLength(1, ErrorMessage = "กรุณาเลือกรายการสิ่งของอย่างน้อย 1 รายการ")]
        public List<CreateDonationItemDto> Items { get; set; }
            = new();
    }
}