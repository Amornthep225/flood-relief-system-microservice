using FloodRelief.Data;
using FloodRelief.DTOs.Staff;
using FloodRelief.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/staffs")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffs()
        {
            var staffs = await _context.Staffs
                .Include(x => x.Center)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CenterId,
                    centerName = x.Center != null ? x.Center.CenterName : null,
                    x.FullName,
                    x.Username,
                    x.Email,
                    x.PhoneNumber,
                    x.Role,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(staffs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(string id)
        {
            var staff = await _context.Staffs
                .Include(x => x.Center)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CenterId,
                    centerName = x.Center != null ? x.Center.CenterName : null,
                    x.FullName,
                    x.Username,
                    x.Email,
                    x.PhoneNumber,
                    x.Role,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (staff == null)
                return NotFound(new { message = "ไม่พบเจ้าหน้าที่" });

            return Ok(staff);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetStaffSummary()
        {
            var totalStaff = await _context.Staffs.CountAsync();
            var activeStaff = await _context.Staffs.CountAsync(x => x.IsActive);
            var inactiveStaff = await _context.Staffs.CountAsync(x => !x.IsActive);

            return Ok(new
            {
                totalStaff,
                activeStaff,
                inactiveStaff
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff(CreateStaffRequestDto dto)
        {
            var centerExists = await _context.Centers.AnyAsync(x => x.Id == dto.CenterId);

            if (!centerExists)
                return BadRequest(new { message = "ไม่พบศูนย์ที่เลือก" });

            var exists = await _context.Staffs.AnyAsync(x =>
                x.Username == dto.Username ||
                x.Email == dto.Email ||
                x.PhoneNumber == dto.PhoneNumber
            );

            if (exists)
                return BadRequest(new { message = "Username, Email หรือเบอร์โทรนี้ถูกใช้งานแล้ว" });

            var lastStaff = await _context.Staffs
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            string newId = "1";

            if (lastStaff != null)
                newId = (int.Parse(lastStaff.Id) + 1).ToString();

            var passwordHasher = new PasswordHasher<string>();

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

            staff.PasswordHash = passwordHasher.HashPassword(staff.Email, dto.Password);

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "เพิ่มเจ้าหน้าที่สำเร็จ",
                staffId = staff.Id,
                staff.FullName,
                staff.Username,
                staff.Email,
                staff.CenterId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(string id, UpdateStaffRequestDto dto)
        {
            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
                return NotFound(new { message = "ไม่พบเจ้าหน้าที่" });

            var centerExists = await _context.Centers.AnyAsync(x => x.Id == dto.CenterId);

            if (!centerExists)
                return BadRequest(new { message = "ไม่พบศูนย์ที่เลือก" });

            var duplicate = await _context.Staffs.AnyAsync(x =>
                x.Id != id &&
                (
                    x.Username == dto.Username ||
                    x.Email == dto.Email ||
                    x.PhoneNumber == dto.PhoneNumber
                )
            );

            if (duplicate)
                return BadRequest(new { message = "Username, Email หรือเบอร์โทรนี้ถูกใช้งานแล้ว" });

            staff.CenterId = dto.CenterId;
            staff.FullName = dto.FullName;
            staff.Username = dto.Username;
            staff.Email = dto.Email;
            staff.PhoneNumber = dto.PhoneNumber;
            staff.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "แก้ไขข้อมูลเจ้าหน้าที่สำเร็จ" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStaffStatus(string id, UpdateStaffStatusDto dto)
        {
            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
                return NotFound(new { message = "ไม่พบเจ้าหน้าที่" });

            staff.IsActive = dto.IsActive;
            staff.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "อัปเดตสถานะเจ้าหน้าที่สำเร็จ" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
                return NotFound(new { message = "ไม่พบเจ้าหน้าที่" });

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ลบเจ้าหน้าที่สำเร็จ" });
        }
    }
}