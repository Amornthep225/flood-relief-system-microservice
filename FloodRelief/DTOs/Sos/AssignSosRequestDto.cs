using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class AssignSosRequestDto
    {
        [StringLength(5, ErrorMessage = "รหัสศูนย์ต้องไม่เกิน 5 ตัวอักษร")]
        public string? CenterId { get; set; }

        [StringLength(10, ErrorMessage = "รหัสเจ้าหน้าที่ต้องไม่เกิน 5 ตัวอักษร")]
        public string? StaffId { get; set; }

        [Required(ErrorMessage = "กรุณาระบุระดับความเร่งด่วน")]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [StringLength(500, ErrorMessage = "หมายเหตุเจ้าหน้าที่ต้องไม่เกิน 500 ตัวอักษร")]
        public string? StaffRemark { get; set; }
    }
}
