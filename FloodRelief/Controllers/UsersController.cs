using FloodRelief.Data;
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
        public async Task<IActionResult> GetAllUsers()
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
                return NotFound(new { message = "ไม่พบผู้ใช้งาน" });

            return Ok(user);
        }
    }
}