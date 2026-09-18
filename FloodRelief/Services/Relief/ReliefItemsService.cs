using FloodRelief.Helpers;
using System.Data;
using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services
{
    public class ReliefItemsService : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReliefItemsService(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/relief-items
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
                    x.MaximumRequestQuantity,
                    x.IsActive,
                    x.IsDonationOpen,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/relief-items/active
        public async Task<IActionResult> GetActiveItems(
    string? centerId = null
)
        {
            // ระบบใช้ศูนย์เดียว
            // ถ้าไม่ได้ส่ง centerId มา ให้เลือกศูนย์ที่เปิดใช้งานอยู่โดยอัตโนมัติ
            var effectiveCenterId = centerId;

            if (string.IsNullOrWhiteSpace(effectiveCenterId))
            {
                effectiveCenterId =
                    await _context.Centers
                        .AsNoTracking()
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.Id)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync();
            }

            var hasCenter =
                !string.IsNullOrWhiteSpace(effectiveCenterId);

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

                    CategoryName =
                        x.ReliefCategory!.Name,

                    x.Name,
                    x.Unit,
                    x.MaximumRequestQuantity,
                    x.IsDonationOpen,

                    // จำนวนที่มีจริงในคลัง
                    CurrentQuantity =
                        hasCenter
                            ? (
                                _context.CenterInventories
                                    .Where(i =>
                                        i.CenterId == effectiveCenterId &&
                                        i.ReliefItemId == x.Id
                                    )
                                    .Select(i =>
                                        (int?)i.Quantity
                                    )
                                    .FirstOrDefault()
                                ?? 0
                            )
                            : 0,

                    // จำนวนสูงสุดที่ Admin กำหนด
                    MaximumQuantity =
                        hasCenter
                            ? (
                                _context.CenterInventories
                                    .Where(i =>
                                        i.CenterId == effectiveCenterId &&
                                        i.ReliefItemId == x.Id
                                    )
                                    .Select(i =>
                                        (int?)i.MaximumQuantity
                                    )
                                    .FirstOrDefault()
                                ?? 0
                            )
                            : 0,

                    // จำนวนบริจาคที่กำลัง Pending
                    PendingQuantity =
                        hasCenter
                            ? (
                                _context.Donations
                                    .Where(d =>
                                        d.CenterId == effectiveCenterId &&
                                        d.Status == "Pending"
                                    )
                                    .SelectMany(d =>
                                        d.Items
                                    )
                                    .Where(di =>
                                        di.ReliefItemId == x.Id
                                    )
                                    .Sum(di =>
                                        (int?)di.Quantity
                                    )
                                ?? 0
                            )
                            : 0
                })
                .ToListAsync();

            var result = items
                .Select(x =>
                {
                    int? remainingQuantity = null;

                    // MaximumQuantity = 0 หมายถึง ไม่จำกัด
                    if (x.MaximumQuantity > 0)
                    {
                        remainingQuantity =
                            Math.Max(
                                0,
                                x.MaximumQuantity -
                                x.CurrentQuantity -
                                x.PendingQuantity
                            );
                    }

                    // บริจาคได้เมื่อ
                    // 1. Admin เปิดรับ
                    // 2. ยังไม่ถึงจำนวนสูงสุด
                    var canDonate =
                        x.IsDonationOpen &&
                        (
                            x.MaximumQuantity == 0 ||
                            remainingQuantity > 0
                        );

                    return new
                    {
                        x.Id,
                        x.ReliefCategoryId,
                        x.CategoryName,
                        x.Name,
                        x.Unit,
                        x.MaximumRequestQuantity,

                        x.IsDonationOpen,

                        x.CurrentQuantity,
                        x.PendingQuantity,
                        x.MaximumQuantity,

                        RemainingQuantity =
                            remainingQuantity,

                        CanDonate =
                            canDonate
                    };
                })
                .ToList();

            return Ok(result);
        }

        // GET: api/relief-items/category/food
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
                    x.Unit,
                    x.MaximumRequestQuantity
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/relief-items/00001
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
                    x.MaximumRequestQuantity,
                    x.IsActive,
                    x.IsDonationOpen,
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
        public async Task<IActionResult> CreateItem(
           CreateReliefItemDto dto)
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
                 x.Name == normalizedName &&
                 x.Unit == normalizedUnit
             );

            if (duplicate)
            {
                return BadRequest(new
                {
                    message =
                        $"มีรายการ {normalizedName} หน่วย {normalizedUnit} อยู่แล้ว"
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            try
            {
                var newId = await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.ReliefItems,
                    x => x.Id,
                    "ReliefItem");

                var item = new ReliefItem
                {
                    Id = newId,
                    ReliefCategoryId = normalizedCategoryId,
                    Name = normalizedName,
                    Unit = normalizedUnit,
                    MaximumRequestQuantity = dto.MaximumRequestQuantity,
                    IsActive = true,
                    IsDonationOpen = dto.IsDonationOpen,
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
                            item.MaximumRequestQuantity,
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
        public async Task<IActionResult> UpdateItem(
            string id,
           UpdateReliefItemDto dto)
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
                x.Name == normalizedName &&
                x.Unit == normalizedUnit
            );

            if (duplicate)
            {
                return BadRequest(new
                {
                    message =
                        $"มีรายการ {normalizedName} หน่วย {normalizedUnit} อยู่แล้ว"
                });
            }

            item.ReliefCategoryId = normalizedCategoryId;
            item.Name = normalizedName;
            item.Unit = normalizedUnit;
            item.MaximumRequestQuantity = dto.MaximumRequestQuantity;
            item.IsDonationOpen = dto.IsDonationOpen;
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
                    item.MaximumRequestQuantity,
                    item.IsActive,
                    item.CreatedAt
                }
            });
        }

        // PUT: api/relief-items/00001/status
        public async Task<IActionResult> UpdateItemStatus(
            string id,
           UpdateReliefItemStatusDto dto)
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
        public async Task<IActionResult> UpdateDonationStatus(
            string id,
            UpdateReliefItemDonationStatusDto dto
        )
        {
            var item = await _context.ReliefItems.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            if (dto.IsDonationOpen && !item.IsActive)
            {
                return BadRequest(new
                {
                    message = "ไม่สามารถเปิดรับบริจาคได้ เพราะรายการสิ่งของนี้ถูกปิดใช้งานอยู่"
                });
            }

            item.IsDonationOpen = dto.IsDonationOpen;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.IsDonationOpen
                    ? "เปิดรับบริจาครายการนี้แล้ว"
                    : "ปิดรับบริจาครายการนี้แล้ว",

                data = new
                {
                    item.Id,
                    item.IsDonationOpen
                }
            });
        }

        // DELETE: api/relief-items/00001
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

    }
}
