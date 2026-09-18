using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Center
{
    public class UpdateInventoryThresholdsDto
    {
        [Range(0, int.MaxValue)]
        [DefaultValue("0")]
        public int MinimumQuantity { get; set; }

        [Range(0, int.MaxValue)]
        [DefaultValue("0")]
        public int MaximumQuantity { get; set; }
    }
}
