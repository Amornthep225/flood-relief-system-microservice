using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Relief
{
    public class UpdateReliefCategoryDto
    {
        [Required(ErrorMessage = "กรุณาระบุชื่อหมวดหมู่")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = string.Empty;
    }
}