using FloodRelief.Services.Common;
using FloodRelief.Helpers;
using System.Security.Claims;
using FloodRelief.Data;
using FloodRelief.DTOs.Center;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services.Center
{
    public class CenterInventoriesService : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUser;

        public CenterInventoriesService(
            AppDbContext context,
            CurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // ดูของทั้งหมดในศูนย์
        // GET /api/inventories/center/00001
        public async Task<IActionResult> GetCenterInventory(
    string centerId)
        {
            var centerExists =
                await _context.Centers
                    .AnyAsync(x =>
                        x.Id == centerId &&
                        x.IsActive
                    );

            if (!centerExists)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์ช่วยเหลือ"
                });
            }

            var inventories =
                await _context.CenterInventories
                    .Where(x =>
                        x.CenterId == centerId
                    )
                    .OrderBy(x =>
                        x.ReliefItem.Name
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.CenterId,

                        centerName =
                            x.Center.CenterName,

                        x.ReliefItemId,

                        reliefItemName =
                            x.ReliefItem.Name,

                        // เพิ่มตรงนี้
                        categoryName =
                            x.ReliefItem.ReliefCategory != null
                                ? x.ReliefItem.ReliefCategory.Name
                                : "อื่น ๆ",

                        categoryId =
                            x.ReliefItem.ReliefCategoryId,

                        unit =
                            x.ReliefItem.Unit,

                        x.Quantity,
                        x.MinimumQuantity,
                        x.MaximumQuantity,
                        stockStatus =
                            x.Quantity <= 0
                                ? "OutOfStock"
                                : x.Quantity <=
                                  x.MinimumQuantity
                                    ? "LowStock"
                                    : "Available",

                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .ToListAsync();

            return Ok(inventories);
        }

        // ดูของรายการเดียวในศูนย์
        // GET /api/inventories/center/00001/item/0000000001
        //"center/{centerId}/item/{reliefItemId}"
        //)]
        public async Task<IActionResult>
            GetCenterInventoryItem(
                string centerId,
                string reliefItemId)
        {
            var inventory =
                await _context.CenterInventories
                    .Where(x =>
                        x.CenterId == centerId &&
                        x.ReliefItemId ==
                            reliefItemId
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.CenterId,
                        centerName =
                            x.Center.CenterName,

                        x.ReliefItemId,
                        reliefItemName =
                            x.ReliefItem.Name,

                        unit =
                            x.ReliefItem.Unit,

                        x.Quantity,
                        x.MinimumQuantity,

                        stockStatus =
                            x.Quantity <= 0
                                ? "OutOfStock"
                                : x.Quantity <=
                                  x.MinimumQuantity
                                    ? "LowStock"
                                    : "Available",

                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return NotFound(new
                {
                    message =
                        "ไม่พบรายการสิ่งของในศูนย์นี้"
                });
            }

            return Ok(inventory);
        }

        // เพิ่มของเข้าศูนย์
        // POST /api/inventories/stock-in
        public async Task<IActionResult> StockIn(
            [FromBody]
            UpdateInventoryQuantityDto dto)
        {
            if (dto.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "จำนวนสิ่งของต้องมากกว่า 0"
                });
            }

            var center =
                await _context.Centers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CenterId &&
                        x.IsActive
                    );

            if (center == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์ช่วยเหลือ"
                });
            }

            var reliefItem =
                await _context.ReliefItems
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.ReliefItemId
                    );

            if (reliefItem == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการสิ่งของ"
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var inventory =
                    await _context.CenterInventories
                        .FirstOrDefaultAsync(x =>
                            x.CenterId ==
                                dto.CenterId &&
                            x.ReliefItemId ==
                                dto.ReliefItemId
                        );

                if (inventory == null)
                {
                    inventory =
                        new CenterInventory
                        {
                            Id =
                                await PrimaryKeyHelper.GenerateNextIdAsync(
                                    _context.CenterInventories,
                                    x => x.Id,
                                    "CenterInventory"),

                            CenterId =
                                dto.CenterId,

                            ReliefItemId =
                                dto.ReliefItemId,

                            Quantity = 0,

                            MinimumQuantity = 0,

                            CreatedAt =
                                DateTime.Now
                        };

                    _context.CenterInventories
                        .Add(inventory);
                }

                inventory.Quantity +=
                    dto.Quantity;

                inventory.UpdatedAt =
                    DateTime.Now;

                var staffId =
                    _currentUser.UserId;

                var inventoryTransaction =
                    new InventoryTransaction
                    {
                        Id =
                            await PrimaryKeyHelper.GenerateNextIdAsync(
                                _context.InventoryTransactions,
                                x => x.Id,
                                "InventoryTransaction"),

                        CenterInventoryId =
                            inventory.Id,

                        TransactionType =
                            string.IsNullOrWhiteSpace(
                                dto.TransactionType
                            )
                                ? "ManualIn"
                                : dto.TransactionType,

                        Quantity =
                            dto.Quantity,

                        BalanceAfter =
                            inventory.Quantity,

                        ReferenceType =
                            dto.ReferenceType,

                        ReferenceId =
                            dto.ReferenceId,

                        Note =
                            dto.Note,

                        StaffId =
                            staffId,

                        CreatedAt =
                            DateTime.Now
                    };

                _context.InventoryTransactions
                    .Add(inventoryTransaction);

                await _context
                    .SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message =
                        "เพิ่มสิ่งของเข้าศูนย์สำเร็จ",

                    inventoryId =
                        inventory.Id,

                    centerId =
                        inventory.CenterId,

                    reliefItemId =
                        inventory.ReliefItemId,

                    quantityAdded =
                        dto.Quantity,

                    currentQuantity =
                        inventory.Quantity
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "ไม่สามารถเพิ่มสิ่งของเข้าศูนย์ได้"
                });
            }
        }

        // นำของออกจากศูนย์
        // POST /api/inventories/stock-out
        public async Task<IActionResult> StockOut(
            [FromBody]
            UpdateInventoryQuantityDto dto)
        {
            if (dto.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "จำนวนสิ่งของต้องมากกว่า 0"
                });
            }

            var inventory =
                await _context.CenterInventories
                    .FirstOrDefaultAsync(x =>
                        x.CenterId ==
                            dto.CenterId &&
                        x.ReliefItemId ==
                            dto.ReliefItemId
                    );

            if (inventory == null)
            {
                return NotFound(new
                {
                    message =
                        "ไม่พบรายการสิ่งของในศูนย์"
                });
            }

            if (inventory.Quantity <
                dto.Quantity)
            {
                return BadRequest(new
                {
                    message =
                        "จำนวนสิ่งของในคลังไม่เพียงพอ",

                    currentQuantity =
                        inventory.Quantity,

                    requestedQuantity =
                        dto.Quantity
                });
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                inventory.Quantity -=
                    dto.Quantity;

                inventory.UpdatedAt =
                    DateTime.Now;

                var staffId =
                    _currentUser.UserId;

                var inventoryTransaction =
                    new InventoryTransaction
                    {
                        Id =
                            await PrimaryKeyHelper.GenerateNextIdAsync(
                                _context.InventoryTransactions,
                                x => x.Id,
                                "InventoryTransaction"),

                        CenterInventoryId =
                            inventory.Id,

                        TransactionType =
                            string.IsNullOrWhiteSpace(
                                dto.TransactionType
                            )
                                ? "ManualOut"
                                : dto.TransactionType,

                        Quantity =
                            dto.Quantity,

                        BalanceAfter =
                            inventory.Quantity,

                        ReferenceType =
                            dto.ReferenceType,

                        ReferenceId =
                            dto.ReferenceId,

                        Note =
                            dto.Note,

                        StaffId =
                            staffId,

                        CreatedAt =
                            DateTime.Now
                    };

                _context.InventoryTransactions
                    .Add(inventoryTransaction);

                await _context
                    .SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message =
                        "นำสิ่งของออกจากศูนย์สำเร็จ",

                    inventoryId =
                        inventory.Id,

                    quantityRemoved =
                        dto.Quantity,

                    currentQuantity =
                        inventory.Quantity
                });
            }
            catch
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "ไม่สามารถนำสิ่งของออกจากศูนย์ได้"
                });
            }
        }

        // แก้ไขจำนวนขั้นต่ำ
        // PUT /api/inventories/0000000001/minimum
        public async Task<IActionResult>
            UpdateMinimumQuantity(
                string id,
                [FromBody]
                UpdateMinimumQuantityDto dto)
        {
            var inventory =
                await _context.CenterInventories
                    .FindAsync(id);

            if (inventory == null)
            {
                return NotFound(new
                {
                    message =
                        "ไม่พบรายการคลังสิ่งของ"
                });
            }

            inventory.MinimumQuantity =
                dto.MinimumQuantity;

            inventory.UpdatedAt =
                DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "แก้ไขจำนวนขั้นต่ำสำเร็จ",

                inventory.Id,

                inventory.MinimumQuantity
            });
        }

        // ดูประวัติการเพิ่มและลด
        // GET /api/inventories/0000000001/transactions
        public async Task<IActionResult>
            GetTransactions(string id)
        {
            var inventoryExists =
                await _context.CenterInventories
                    .AnyAsync(x => x.Id == id);

            if (!inventoryExists)
            {
                return NotFound(new
                {
                    message =
                        "ไม่พบรายการคลังสิ่งของ"
                });
            }

            var transactions =
                await _context
                    .InventoryTransactions
                    .Where(x =>
                        x.CenterInventoryId == id
                    )
                    .OrderByDescending(x =>
                        x.CreatedAt
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.CenterInventoryId,
                        x.TransactionType,
                        x.Quantity,
                        x.BalanceAfter,
                        x.ReferenceType,
                        x.ReferenceId,
                        x.Note,
                        x.StaffId,
                        staffName =
                            x.Staff != null
                                ? x.Staff.FullName
                                : null,
                        x.CreatedAt
                    })
                    .ToListAsync();

            return Ok(transactions);
        }

        // ดูรายการของขาด
        // GET /api/inventories/low-stock
        public async Task<IActionResult>
            GetLowStock()
        {
            var inventories =
                await _context.CenterInventories
                    .Where(x =>
                        x.Quantity <=
                        x.MinimumQuantity
                    )
                    .OrderBy(x =>
                        x.Quantity
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.CenterId,
                        centerName =
                            x.Center.CenterName,

                        x.ReliefItemId,
                        reliefItemName =
                            x.ReliefItem.Name,

                        unit =
                            x.ReliefItem.Unit,

                        x.Quantity,
                        x.MinimumQuantity,
                        x.MaximumQuantity,
                        stockStatus =
                            x.Quantity <= 0
                                ? "OutOfStock"
                                : "LowStock"
                    })
                    .ToListAsync();

            return Ok(inventories);
        }
        public async Task<IActionResult> UpdateThresholds(
            string id,
            UpdateInventoryThresholdsDto dto
            )
        {
            var inventory =
                await _context.CenterInventories.FindAsync(id);

            if (inventory == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบรายการคลังสิ่งของ"
                });
            }

            if (
                dto.MaximumQuantity > 0 &&
                dto.MaximumQuantity < dto.MinimumQuantity
            )
            {
                return BadRequest(new
                {
                    message =
                        "จำนวนสูงสุดต้องมากกว่าหรือเท่ากับจำนวนขั้นต่ำ"
                });
            }

            inventory.MinimumQuantity =
                dto.MinimumQuantity;

            inventory.MaximumQuantity =
                dto.MaximumQuantity;

            inventory.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "แก้ไขจำนวนขั้นต่ำและจำนวนสูงสุดสำเร็จ",

                data = new
                {
                    inventory.Id,
                    inventory.Quantity,
                    inventory.MinimumQuantity,
                    inventory.MaximumQuantity
                }
            });
        }

    }

}
