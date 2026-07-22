using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Relief
{
    public class UpdateReliefItemDto
    {
        [Required]
        public string ReliefCategoryId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Unit { get; set; } = string.Empty;
    }
}