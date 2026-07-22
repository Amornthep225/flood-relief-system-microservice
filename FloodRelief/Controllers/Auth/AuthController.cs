using FloodRelief.Data;
using FloodRelief.DTOs.Auth;
using FloodRelief.DTOs.Staff;
using FloodRelief.Models;
using FloodRelief.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;


        public AuthController(
            AppDbContext context,
            JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("user-register")]
        public async Task<IActionResult> UserRegister(
            [FromBody] UserRegisterRequestDto dto)
        {

            var exists = await _context.Users.AnyAsync(x =>
                x.Email == dto.Email ||
                x.PhoneNumber == dto.PhoneNumber
            );


            if (exists)
            {
                return BadRequest(new
                {
                    message = "อีเมลหรือเบอร์โทรนี้ถูกใช้งานแล้ว"
                });
            }



            var lastUser = await _context.Users
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();


            int nextId = 1;


            if (lastUser != null)
            {
                nextId = int.Parse(lastUser.Id) + 1;
            }


            string newId = nextId
                .ToString()
                .PadLeft(10, '0');



            var user = new User
            {
                Id = newId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.Now
            };



            var passwordHasher =
                new PasswordHasher<User>();


            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    dto.Password
                );



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
        public async Task<IActionResult> UserLogin(
            [FromBody] UserLoginRequestDto dto)
        {

            var loginValue =
                dto.PhoneOrEmail.Trim();



            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == loginValue ||
                    x.PhoneNumber == loginValue
                );



            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "ไม่พบบัญชีผู้ใช้งาน"
                });
            }



            if (!user.IsActive)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                        "บัญชีผู้ใช้งานถูกระงับ"
                    }
                );
            }



            var passwordHasher =
                new PasswordHasher<User>();



            var verify =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password
                );



            if (verify ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message =
                    "รหัสผ่านไม่ถูกต้อง"
                });
            }



            var token =
                _jwtService.GenerateUserToken(user);



            return Ok(
                new UserLoginResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                });
        }


        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin(
            [FromBody] StaffLoginRequestDto dto)
        {

            var staff =
                await _context.Staffs
                .FirstOrDefaultAsync(x =>
                    x.Username == dto.UsernameOrEmail ||
                    x.Email == dto.UsernameOrEmail
                );



            if (staff == null)
            {
                return Unauthorized(new
                {
                    message =
                    "ไม่พบบัญชีเจ้าหน้าที่"
                });
            }



            if (!staff.IsActive)
            {
                return Unauthorized(new
                {
                    message =
                    "บัญชีเจ้าหน้าที่ถูกปิดใช้งาน"
                });
            }



            var passwordHasher =
                new PasswordHasher<string>();



            var verify =
                passwordHasher.VerifyHashedPassword(
                    staff.Email,
                    staff.PasswordHash,
                    dto.Password
                );



            if (verify ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message =
                    "รหัสผ่านไม่ถูกต้อง"
                });
            }



            var token =
                _jwtService.GenerateStaffToken(staff);



            return Ok(new
            {
                token,
                staffId = staff.Id,
                fullName = staff.FullName,
                role = staff.Role,
                centerId = staff.CenterId
            });
        }

        [HttpPost("staff-register")]
        public async Task<IActionResult> StaffRegister(
            [FromBody] CreateStaffRequestDto dto)
        {

            var centerExists =
                await _context.Centers
                .AnyAsync(x => x.Id == dto.CenterId);



            if (!centerExists)
            {
                return BadRequest(new
                {
                    message =
                    "ไม่พบศูนย์ที่เลือก"
                });
            }



            var exists =
                await _context.Staffs
                .AnyAsync(x =>
                    x.Email == dto.Email ||
                    x.Username == dto.Username ||
                    x.PhoneNumber == dto.PhoneNumber
                );



            if (exists)
            {
                return BadRequest(new
                {
                    message =
                    "ชื่อผู้ใช้ อีเมล หรือเบอร์โทรนี้ถูกใช้งานแล้ว"
                });
            }



            var lastStaff =
                await _context.Staffs
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();



            int nextId = 1;


            if (lastStaff != null)
            {
                nextId =
                    int.Parse(lastStaff.Id) + 1;
            }



            string newId =
                nextId
                .ToString()
                .PadLeft(5, '0');



            var staff = new Staff
            {
                Id = newId,
                CenterId = dto.CenterId,
                FullName = dto.FullName,
                Username = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = "Staff",
                IsActive = true,
                CreatedAt = DateTime.Now
            };



            var passwordHasher =
                new PasswordHasher<string>();


            staff.PasswordHash =
                passwordHasher.HashPassword(
                    staff.Email,
                    dto.Password
                );



            _context.Staffs.Add(staff);

            await _context.SaveChangesAsync();



            return Ok(new
            {
                message =
                "สร้างบัญชีเจ้าหน้าที่สำเร็จ",

                staffId = staff.Id,

                fullName = staff.FullName,

                role = staff.Role,

                centerId = staff.CenterId
            });
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin(
            [FromBody] AdminLoginRequestDto dto)
        {

            var admin =
                await _context.Admins
                .FirstOrDefaultAsync(x =>
                    x.Username == dto.UsernameOrEmail ||
                    x.Email == dto.UsernameOrEmail
                );



            if (admin == null)
            {
                return Unauthorized(new
                {
                    message =
                    "ไม่พบบัญชีผู้ดูแลระบบ"
                });
            }



            if (!admin.IsActive)
            {
                return Unauthorized(new
                {
                    message =
                    "บัญชีผู้ดูแลระบบถูกปิดใช้งาน"
                });
            }



            var passwordHasher =
                new PasswordHasher<string>();



            var verify =
                passwordHasher.VerifyHashedPassword(
                    admin.Email,
                    admin.PasswordHash,
                    dto.Password
                );



            if (verify ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message =
                    "รหัสผ่านไม่ถูกต้อง"
                });
            }



            var token =
                _jwtService.GenerateAdminToken(admin);



            return Ok(new
            {
                token,
                adminId = admin.Id,
                username = admin.Username,
                email = admin.Email,
                role = admin.Role
            });
        }

        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister(
            [FromBody] AdminRegisterRequestDto dto)
        {

            var exists =
                await _context.Admins
                .AnyAsync(x =>
                    x.Username == dto.Username ||
                    x.Email == dto.Email
                );



            if (exists)
            {
                return BadRequest(new
                {
                    message =
                    "Username หรือ Email ถูกใช้งานแล้ว"
                });
            }



            var lastAdmin =
                await _context.Admins
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();



            int nextId = 1;



            if (lastAdmin != null)
            {
                nextId =
                    int.Parse(lastAdmin.Id) + 1;
            }



            string newId =
                nextId
                .ToString()
                .PadLeft(2, '0');



            var admin = new Admin
            {
                Id = newId,
                Username = dto.Username,
                Email = dto.Email,
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };



            var passwordHasher =
                new PasswordHasher<string>();


            admin.PasswordHash =
                passwordHasher.HashPassword(
                    admin.Email,
                    dto.Password
                );



            _context.Admins.Add(admin);

            await _context.SaveChangesAsync();



            return Ok(new
            {
                message =
                "สร้างผู้ดูแลระบบสำเร็จ",

                adminId = admin.Id,

                username = admin.Username
            });
        }
    }
}