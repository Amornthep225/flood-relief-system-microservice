using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Center
{
    public class UpdateMinimumQuantityDto
    {
        [Range(0, int.MaxValue)]
        public int MinimumQuantity { get; set; }
    }
}