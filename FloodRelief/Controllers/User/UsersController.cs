using FloodRelief.Data;
using FloodRelief.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.PhoneNumber,
                    x.Email,
                    x.Role,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.PhoneNumber,
                    x.Email,
                    x.Role,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "ไม่พบผู้ใช้" });

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserRequestDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "ไม่พบผู้ใช้" });

            var duplicate = await _context.Users.AnyAsync(x =>
                x.Id != id &&
                (x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber)
            );

            if (duplicate)
                return BadRequest(new { message = "อีเมลหรือเบอร์โทรนี้ถูกใช้งานแล้ว" });

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "แก้ไขข้อมูลผู้ใช้สำเร็จ" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(string id, UpdateUserStatusDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "ไม่พบผู้ใช้" });

            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "อัปเดตสถานะผู้ใช้สำเร็จ" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "ไม่พบผู้ใช้" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ลบผู้ใช้สำเร็จ" });
        }
    }
}