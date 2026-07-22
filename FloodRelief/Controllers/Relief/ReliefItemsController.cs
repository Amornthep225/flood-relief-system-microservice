using System.Data;
using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/relief-items")]
    [ApiController]
    public class ReliefItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReliefItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/relief-items
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.ReliefItems
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.ReliefCategoryId,
                    CategoryName = x.ReliefCategory != null
                        ? x.ReliefCategory.Name
                        : null,
                    x.Name,
                    x.Unit,
                    x.IsActive,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/relief-items/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveItems()
        {
            var items = await _context.ReliefItems
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ReliefCategory != null &&
                    x.ReliefCategory.IsActive
                )
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.ReliefCategoryId,
                    CategoryName = x.ReliefCategory!.Name,
                    x.Name,
                    x.Unit
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/relief-items/category/food
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetItemsByCategory(string categoryId)
        {
            var normalizedCategoryId = categoryId.Trim().ToLower();

            var categoryExists = await _context.ReliefCategories
                .AsNoTracking()
                .AnyAsync(x => x.Id == normalizedCategoryId);

            if (!categoryExists)
            {
                return NotFound(new
                {
                    message = "ไม่พบหมวดหมู่สิ่งของ"
                });
            }

            var items = await _context.ReliefItems
                .AsNoTracking()
                .Where(x =>
                    x.ReliefCategoryId == normalizedCategoryId &&
                    x.IsActive
                )
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.ReliefCategoryId,
                    x.Name,
                    x.Unit
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/relief-items/00001
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(string id)
        {
            var item = await _context.ReliefItems
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.ReliefCategoryId,
                    CategoryName = x.ReliefCategory != null
                        ? x.ReliefCategory.Name
                        : null,
                    x.Name,
                    x.Unit,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            return Ok(item);
        }

        // POST: api/relief-items
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateItem(
          [FromBody]  CreateReliefItemDto dto)
        {
            var normalizedCategoryId = dto.ReliefCategoryId
                .Trim()
                .ToLower();

            var normalizedName = dto.Name.Trim();
            var normalizedUnit = dto.Unit.Trim();

            var category = await _context.ReliefCategories
                .FirstOrDefaultAsync(x =>
                    x.Id == normalizedCategoryId &&
                    x.IsActive
                );

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "ไม่พบหมวดหมู่ หรือหมวดหมู่นี้ถูกปิดใช้งาน"
                });
            }

            var duplicate = await _context.ReliefItems
                .AnyAsync(x =>
                    x.ReliefCategoryId == normalizedCategoryId &&
                    x.Name == normalizedName
                );

            if (duplicate)
            {
                return BadRequest(new
                {
                    message = "รายการสิ่งของนี้มีอยู่ในหมวดหมู่แล้ว"
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            try
            {
                var newId = await GenerateNextItemIdAsync();

                var item = new ReliefItem
                {
                    Id = newId,
                    ReliefCategoryId = normalizedCategoryId,
                    Name = normalizedName,
                    Unit = normalizedUnit,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.ReliefItems.Add(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetItemById),
                    new { id = item.Id },
                    new
                    {
                        message = "เพิ่มรายการสิ่งของสำเร็จ",
                        data = new
                        {
                            item.Id,
                            item.ReliefCategoryId,
                            CategoryName = category.Name,
                            item.Name,
                            item.Unit,
                            item.IsActive,
                            item.CreatedAt
                        }
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/relief-items/00001
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(
            string id,
           [FromBody] UpdateReliefItemDto dto)
        {
            var item = await _context.ReliefItems.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            var normalizedCategoryId = dto.ReliefCategoryId
                .Trim()
                .ToLower();

            var normalizedName = dto.Name.Trim();
            var normalizedUnit = dto.Unit.Trim();

            var categoryExists = await _context.ReliefCategories
                .AnyAsync(x =>
                    x.Id == normalizedCategoryId &&
                    x.IsActive
                );

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "ไม่พบหมวดหมู่ หรือหมวดหมู่นี้ถูกปิดใช้งาน"
                });
            }

            var duplicate = await _context.ReliefItems
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ReliefCategoryId == normalizedCategoryId &&
                    x.Name == normalizedName
                );

            if (duplicate)
            {
                return BadRequest(new
                {
                    message = "รายการสิ่งของนี้มีอยู่ในหมวดหมู่แล้ว"
                });
            }

            item.ReliefCategoryId = normalizedCategoryId;
            item.Name = normalizedName;
            item.Unit = normalizedUnit;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "แก้ไขรายการสิ่งของสำเร็จ",
                data = new
                {
                    item.Id,
                    item.ReliefCategoryId,
                    item.Name,
                    item.Unit,
                    item.IsActive,
                    item.CreatedAt
                }
            });
        }

        // PUT: api/relief-items/00001/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItemStatus(
            string id,
           [FromBody] UpdateReliefItemStatusDto dto)
        {
            var item = await _context.ReliefItems.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            item.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.IsActive
                    ? "เปิดใช้งานรายการสิ่งของสำเร็จ"
                    : "ปิดใช้งานรายการสิ่งของสำเร็จ",
                data = new
                {
                    item.Id,
                    item.IsActive
                }
            });
        }

        // DELETE: api/relief-items/00001
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            var item = await _context.ReliefItems
                .Include(x => x.SosRequestItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            if (item.SosRequestItems.Count > 0)
            {
                return BadRequest(new
                {
                    message =
                        "ไม่สามารถลบรายการนี้ได้ เนื่องจากมีประวัติคำขอความช่วยเหลือ กรุณาปิดใช้งานแทน"
                });
            }

            _context.ReliefItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ลบรายการสิ่งของสำเร็จ"
            });
        }

        private async Task<string> GenerateNextItemIdAsync()
        {
            var lastItemId = await _context.ReliefItems
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            long nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastItemId))
            {
                if (!long.TryParse(lastItemId, out var lastNumber))
                {
                    throw new InvalidOperationException(
                        $"รหัสรายการสิ่งของล่าสุดไม่ถูกต้อง: {lastItemId}"
                    );
                }

                nextNumber = lastNumber + 1;
            }

            if (nextNumber > 9_999_999_999)
            {
                throw new InvalidOperationException(
                    "รหัสรายการสิ่งของเกินจำนวนสูงสุด 10 หลัก"
                );
            }

            return nextNumber.ToString("D10");
        }
    }
}