using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class CreateSosRequestDto
    {

        [Range(
            -90,
            90,
            ErrorMessage = "Latitude ต้องอยู่ระหว่าง -90 ถึง 90"
        )]
        public double Latitude { get; set; }

        [Range(
            -180,
            180,
            ErrorMessage = "Longitude ต้องอยู่ระหว่าง -180 ถึง 180"
        )]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "กรุณาระบุรายละเอียดที่อยู่")]
        [StringLength(
            500,
            ErrorMessage = "รายละเอียดที่อยู่ต้องไม่เกิน 500 ตัวอักษร"
        )]
        public string AddressDetail { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "หมายเหตุต้องไม่เกิน 500 ตัวอักษร"
        )]
        public string? UserRemark { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกรายการสิ่งของ")]
        [MinLength(
            1,
            ErrorMessage = "กรุณาเลือกรายการสิ่งของอย่างน้อย 1 รายการ"
        )]
        public List<CreateSosRequestItemDto> Items { get; set; } = new();
    }
}