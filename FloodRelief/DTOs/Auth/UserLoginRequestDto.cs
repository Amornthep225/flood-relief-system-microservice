using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class UserLoginRequestDto
    {
        [Required]
        public string PhoneOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}