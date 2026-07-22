using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class UpdateSosStatusDto
    {
        [Required(ErrorMessage = "กรุณาระบุสถานะ")]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "หมายเหตุต้องไม่เกิน 500 ตัวอักษร"
        )]
        public string? StaffRemark { get; set; }
    }
}