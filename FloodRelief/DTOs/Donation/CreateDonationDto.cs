using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Donation
{
    public class CreateDonationDto
    {
        public string? CenterId { get; set; }


        [Required]
        [MinLength(1, ErrorMessage = "กรุณาเลือกรายการสิ่งของอย่างน้อย 1 รายการ")]
        public List<CreateDonationItemDto> Items { get; set; }
            = new();
    }
}
