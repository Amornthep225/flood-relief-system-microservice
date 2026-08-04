using FloodRelief.Helpers;
using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services
{
    public class ReliefCategoriesService : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReliefCategoriesService(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/relief-categories
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
        public async Task<IActionResult> CreateCategory(
           CreateReliefCategoryDto dto)
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
            var newId = await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.ReliefCategories,
                    x => x.Id,
                    "ReliefCategory",
                    digits: 2);

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
        public async Task<IActionResult> UpdateCategory(
            string id,
           UpdateReliefCategoryDto dto)
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
        public async Task<IActionResult> UpdateCategoryStatus(
            string id,
            UpdateReliefItemStatusDto dto)
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
    }
}
