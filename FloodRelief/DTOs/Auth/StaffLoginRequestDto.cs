using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class StaffLoginRequestDto
    {
        [Required]
        [DefaultValue("จิงจิง")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        [DefaultValue("111")]
        public string Password { get; set; } = string.Empty;
    }
}
