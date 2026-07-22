using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Staff
{
    public class CreateStaffRequestDto
    {
        [Required]
        [StringLength(5)]
        public string CenterId { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}