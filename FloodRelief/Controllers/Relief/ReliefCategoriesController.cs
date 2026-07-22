using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/relief-categories")]
    [ApiController]
    public class ReliefCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReliefCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/relief-categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.ReliefCategories
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Icon,
                    x.IsActive,
                    x.CreatedAt,
                    itemCount = x.ReliefItems.Count
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: /api/relief-categories/active
        // สำหรับหน้า User และ Donor
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _context.ReliefCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    title = x.Name,
                    x.Icon
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: /api/relief-categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            var category = await _context.ReliefCategories
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Icon,
                    x.IsActive,
                    x.CreatedAt,
                    items = x.ReliefItems
                        .OrderBy(i => i.Name)
                        .Select(i => new
                        {
                            i.Id,
                            i.Name,
                            i.Unit,
                            i.IsActive
                        })
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบหมวดหมู่สิ่งของ"
                });
            }

            return Ok(category);
        }

        // POST: /api/relief-categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(
          [FromBody]  CreateReliefCategoryDto dto)
        {
            
            var normalizedName = dto.Name.Trim();


              var duplicateName = await _context.ReliefCategories
                .AnyAsync(x => x.Name == normalizedName);

            if (duplicateName)
            {
                return BadRequest(new
                {
                    message = "ชื่อหมวดหมู่นี้มีอยู่แล้ว"
                });
            }
            var newId = await GenerateNewIdAsync();

            var category = new ReliefCategory
            {
                Id = newId,
                Name = normalizedName,
                Icon = dto.Icon.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.ReliefCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                new
                {
                    message = "เพิ่มหมวดหมู่สิ่งของสำเร็จ",
                    data = new
                    {
                        category.Id,
                        category.Name,
                        category.Icon,
                        category.IsActive,
                        category.CreatedAt
                    }
                }
            );
        }

        // PUT: /api/relief-categories/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(
            string id,
           [FromBody] UpdateReliefCategoryDto dto)
        {
            var category = await _context.ReliefCategories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบหมวดหมู่สิ่งของ"
                });
            }

            var normalizedName = dto.Name.Trim();

            var duplicateName = await _context.ReliefCategories
                .AnyAsync(x =>
                    x.Id != id &&
                    x.Name == normalizedName
                );

            if (duplicateName)
            {
                return BadRequest(new
                {
                    message = "ชื่อหมวดหมู่นี้มีอยู่แล้ว"
                });
            }

            category.Name = normalizedName;
            category.Icon = dto.Icon.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "แก้ไขหมวดหมู่สิ่งของสำเร็จ",
                data = new
                {
                    category.Id,
                    category.Name,
                    category.Icon,
                    category.IsActive,
                    category.CreatedAt
                }
            });
        }

        // PUT: /api/relief-categories/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategoryStatus(
            string id,
            [FromBody] UpdateReliefItemStatusDto dto)
        {
            var category = await _context.ReliefCategories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบหมวดหมู่สิ่งของ"
                });
            }

            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.IsActive
                    ? "เปิดใช้งานหมวดหมู่สำเร็จ"
                    : "ปิดใช้งานหมวดหมู่สำเร็จ",
                data = new
                {
                    category.Id,
                    category.IsActive
                }
            });
        }

        // DELETE: /api/relief-categories/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var category = await _context.ReliefCategories
                .Include(x => x.ReliefItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบหมวดหมู่สิ่งของ"
                });
            }

            if (category.ReliefItems.Count > 0)
            {
                return BadRequest(new
                {
                    message = "ไม่สามารถลบหมวดหมู่ได้ เนื่องจากยังมีรายการสิ่งของอยู่ในหมวดนี้ กรุณาปิดใช้งานแทน"
                });
            }

            _context.ReliefCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ลบหมวดหมู่สิ่งของสำเร็จ"
            });
        }
        private async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _context.ReliefCategories
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastId))
            {
                return "01";
            }

            if (!int.TryParse(lastId, out var lastNumber))
            {
                throw new InvalidOperationException(
                    $"รหัส การสร้าง ล่าสุดไม่ถูกต้อง: {lastId}"
                );
            }

            var nextNumber = lastNumber + 1;

            if (nextNumber > 99999999)
            {
                throw new InvalidOperationException(
                    "รหัส การสร้าง เต็มแล้ว"
                );
            }

            return nextNumber.ToString("D2");
        }
    }
}