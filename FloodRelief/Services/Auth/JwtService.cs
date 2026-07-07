using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FloodRelief.Models;
using Microsoft.IdentityModel.Tokens;

namespace FloodRelief.Services.Auth
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateUserToken(User user)
        {
            var jwt = _configuration.GetSection("Jwt");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userType", "user")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwt["ExpireMinutes"])
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateStaffToken(Staff staff)
        {
            var jwt = _configuration.GetSection("Jwt");

            var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier, staff.Id),
        new Claim(ClaimTypes.Name, staff.FullName),
        new Claim(ClaimTypes.Role, staff.Role),
        new Claim("userType", "staff"),
        new Claim("centerId", staff.CenterId)
         };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwt["ExpireMinutes"])
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateAdminToken(Admin admin)
        {
            var jwt = _configuration.GetSection("Jwt");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, admin.Id),
        new Claim(ClaimTypes.Name, admin.Username),
        new Claim(ClaimTypes.Role, admin.Role),
        new Claim("userType", "admin"),
        new Claim("username", admin.Username),
        new Claim("email", admin.Email)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwt["ExpireMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}