using FloodRelief.Data;
using FloodRelief.DTOs.Auth;
using FloodRelief.Models;
using FloodRelief.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("user-register")]
        public async Task<IActionResult> UserRegister(UserRegisterRequestDto dto)
        {
            var exists = await _context.Users.AnyAsync(x =>
                x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber
            );

            if (exists)
                return BadRequest(new { message = "อีเมลหรือเบอร์โทรนี้ถูกใช้งานแล้ว" });

            var passwordHasher = new PasswordHasher<string>();

            var lastUser = await _context.Users
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

            string newId = "1";

            if (lastUser != null)
            {
                newId = (int.Parse(lastUser.Id) + 1).ToString();
            }

            var user = new User
            {
                Id = newId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = passwordHasher.HashPassword(user.Email, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "สมัครสมาชิกสำเร็จ",
                userId = user.Id,
                fullName = user.FullName,
                role = user.Role
            });
        }

        [HttpPost("user-login")]
        public async Task<ActionResult<UserLoginResponseDto>> UserLogin(UserLoginRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Email == dto.PhoneOrEmail ||
                x.PhoneNumber == dto.PhoneOrEmail
            );

            if (user == null)
                return Unauthorized(new { message = "ไม่พบบัญชีผู้ใช้งาน" });

            if (!user.IsActive)
                return Unauthorized(new { message = "บัญชีนี้ถูกปิดใช้งาน" });

            var passwordHasher = new PasswordHasher<string>();

            var verify = passwordHasher.VerifyHashedPassword(
                user.Email,
                user.PasswordHash,
                dto.Password
            );

            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "รหัสผ่านไม่ถูกต้อง" });

            var token = _jwtService.GenerateUserToken(user);

            return Ok(new UserLoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Role = user.Role
            });
        }
    }
}