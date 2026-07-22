using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class AssignSosRequestDto
    {
        [Required(ErrorMessage = "กรุณาเลือกศูนย์ช่วยเหลือ")]
        [StringLength(5,
            ErrorMessage = "รหัสศูนย์ต้องไม่เกิน 3 ตัวอักษร"
        )]
        public string CenterId { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกเจ้าหน้าที่")]
        [StringLength(5,
            ErrorMessage = "รหัสเจ้าหน้าที่ต้องไม่เกิน 4 ตัวอักษร"
        )]
        public string StaffId { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุระดับความเร่งด่วน")]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [StringLength(500,
            ErrorMessage = "หมายเหตุเจ้าหน้าที่ต้องไม่เกิน 500 ตัวอักษร"
        )]
        public string? StaffRemark { get; set; }
    }
}