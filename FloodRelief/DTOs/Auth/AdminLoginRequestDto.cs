using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class AdminLoginRequestDto
    {
        [Required]
        [DefaultValue("admin")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        [DefaultValue("111")]
        public string Password { get; set; } = string.Empty;
    }
}