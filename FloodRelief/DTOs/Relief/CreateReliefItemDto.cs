using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Relief
{
    public class CreateReliefItemDto
    {

        [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
        [StringLength(5)]
        public string ReliefCategoryId { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุชื่อสิ่งของ")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุหน่วย")]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;
    }
}