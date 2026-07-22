using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Auth
{
    public class UserLoginRequestDto
    {
        [Required(ErrorMessage = "กรุณาระบุเบอร์โทรศัพท์หรืออีเมล")]
        [DefaultValue("0000000000")]
        public string PhoneOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุรหัสผ่าน")]
        [DefaultValue("111")]
        public string Password { get; set; } = string.Empty;
    }
}