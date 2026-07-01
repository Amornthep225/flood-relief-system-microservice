namespace FloodRelief.DTOs.Auth
{
    public class UserLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public string UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}