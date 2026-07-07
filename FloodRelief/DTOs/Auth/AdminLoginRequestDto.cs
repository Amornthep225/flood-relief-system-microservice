using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class AdminLoginRequestDto
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}