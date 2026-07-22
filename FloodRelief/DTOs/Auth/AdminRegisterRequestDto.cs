using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class AdminRegisterRequestDto
    {
        [Required]
        [DefaultValue("admin")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [DefaultValue("admin@example.com")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}