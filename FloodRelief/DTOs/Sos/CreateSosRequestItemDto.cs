using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class CreateSosRequestItemDto
    {
        [Required]
        [StringLength(10,
            ErrorMessage = "รหัสของต้องไม่เกิน 10 ตัวอักษร")]
        public string ReliefItemId { get; set; } = string.Empty;


        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}