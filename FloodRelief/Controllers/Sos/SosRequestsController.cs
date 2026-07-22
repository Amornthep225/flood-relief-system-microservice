using System.Data;
using System.Security.Claims;
using FloodRelief.Constants;
using FloodRelief.Data;
using FloodRelief.DTOs.Sos;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [Route("api/sos-requests")]
    [ApiController]
    [Authorize]
    public class SosRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SosRequestsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("my-requests")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้"
                });
            }


            var requests = await _context.SosRequests

                .Where(x => x.UserId == userId)

                .Include(x => x.Items)
                    .ThenInclude(i => i.ReliefItem)

                .Include(x => x.Center)

                .Include(x => x.AssignedStaff)


                .OrderByDescending(x => x.CreatedAt)


                .Select(x => new
                {
                    x.Id,

                    x.Status,

                    x.Priority,

                    x.AddressDetail,

                    x.Latitude,

                    x.Longitude,

                    x.UserRemark,

                    x.StaffRemark,


                    x.CreatedAt,

                    x.AcceptedAt,

                    x.PreparingAt,

                    x.DeliveringAt,

                    x.CompletedAt,

                    x.CancelledAt,


                    Center = x.Center == null
                        ? null
                        : new
                        {
                            x.Center.Id,
                            x.Center.CenterName,
                            x.Center.PhoneNumber
                        },


                    Staff = x.AssignedStaff == null
                        ? null
                        : new
                        {
                            x.AssignedStaff.Id,
                            x.AssignedStaff.FullName,
                            x.AssignedStaff.PhoneNumber
                        },


                    Items = x.Items.Select(i => new
                    {
                        i.ReliefItemId,

                        Name = i.ReliefItem.Name,

                        i.Quantity,

                        i.Unit
                    })

                })

                .ToListAsync();


            return Ok(requests);
        }
        // POST: api/sos-requests
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateSosRequest(
          [FromBody]  CreateSosRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้จาก Token"
                });
            }

            var userExists = await _context.Users
                .AnyAsync(x => x.Id == userId && x.IsActive);

            if (!userExists)
            {
                return Unauthorized(new
                {
                    message = "ไม่พบบัญชีผู้ใช้ หรือบัญชีถูกระงับ"
                });
            }

            if (dto.Items.Count == 0)
            {
                return BadRequest(new
                {
                    message = "กรุณาเลือกรายการสิ่งของอย่างน้อย 1 รายการ"
                });
            }

            var duplicateItemIds = dto.Items
                .GroupBy(x => x.ReliefItemId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateItemIds.Count > 0)
            {
                return BadRequest(new
                {
                    message = "พบรายการสิ่งของซ้ำในคำขอ"
                });
            }

            var itemIds = dto.Items
                .Select(x => x.ReliefItemId)
                .ToList();

            var reliefItems = await _context.ReliefItems
                .Where(x =>
                    itemIds.Contains(x.Id) &&
                    x.IsActive &&
                    x.ReliefCategory != null &&
                    x.ReliefCategory.IsActive
                )
                .ToListAsync();

            if (reliefItems.Count != itemIds.Count)
            {
                return BadRequest(new
                {
                    message = "มีรายการสิ่งของบางรายการไม่ถูกต้องหรือถูกปิดใช้งาน"
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            try
            {
                var requestId = await GenerateNextSosRequestIdAsync();

                var request = new SosRequest
                {
                    Id = requestId,
                    UserId = userId,
                    CenterId = null,
                    AssignedStaffId = null,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    AddressDetail = dto.AddressDetail.Trim(),
                    UserRemark = dto.UserRemark?.Trim(),
                    Priority = SosPriorities.Normal,
                    Status = SosRequestStatuses.Pending,
                    CreatedAt = DateTime.Now
                };

                var nextItemNumber =
                    await GetNextSosRequestItemNumberAsync();

                foreach (var dtoItem in dto.Items)
                {
                    var reliefItem = reliefItems
                        .First(x => x.Id == dtoItem.ReliefItemId);

                    request.Items.Add(new SosRequestItem
                    {
                        Id = nextItemNumber.ToString("D8"),
                        SosRequestId = request.Id,
                        ReliefItemId = reliefItem.Id,
                        Quantity = dtoItem.Quantity,
                        Unit = reliefItem.Unit,
                        CreatedAt = DateTime.Now
                    });

                    nextItemNumber++;
                }

                _context.SosRequests.Add(request);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetSosRequestById),
                    new { id = request.Id },
                    new
                    {
                        message = "ส่งคำขอความช่วยเหลือสำเร็จ",
                        sosRequestId = request.Id,
                        status = request.Status,
                        createdAt = request.CreatedAt
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // GET: api/sos-requests/my
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMySosRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้จาก Token"
                });
            }

            var requests = await _context.SosRequests
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SosRequestListDto
                {
                    Id = x.Id,
                    UserId = x.UserId,

                    UserFullName = x.User != null
                        ? x.User.FullName
                        : string.Empty,

                    UserPhoneNumber = x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,

                    CenterId = x.CenterId,

                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : null,

                    AssignedStaffId = x.AssignedStaffId,

                    AssignedStaffName = x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AddressDetail = x.AddressDetail,
                    Priority = x.Priority,
                    Status = x.Status,

                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/sos-requests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllSosRequests(
            [FromQuery] string? status,
            [FromQuery] string? centerId)
        {
            var query = _context.SosRequests
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(centerId))
            {
                query = query.Where(x => x.CenterId == centerId);
            }

            var requests = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    UserName = x.User != null
                        ? x.User.FullName
                        : "",
                    UserPhone = x.User != null
                        ? x.User.PhoneNumber
                        : "",
                    x.CenterId,
                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : null,
                    x.AssignedStaffId,
                    StaffName = x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,
                    x.Latitude,
                    x.Longitude,
                    x.AddressDetail,
                    x.Priority,
                    x.Status,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        // GET: api/sos-requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSosRequestById(string id)
        {
            var request = await _context.SosRequests
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new SosRequestDetailDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserFullName = x.User != null ? x.User.FullName : string.Empty,
                    UserPhoneNumber = x.User != null ? x.User.PhoneNumber : string.Empty,
                    UserEmail = x.User != null ? x.User.Email : string.Empty,

                    CenterId = x.CenterId,
                    CenterName = x.Center != null ? x.Center.CenterName : null,
                    CenterPhoneNumber = x.Center != null ? x.Center.PhoneNumber : null,

                    AssignedStaffId = x.AssignedStaffId,
                    AssignedStaffName = x.AssignedStaff != null ? x.AssignedStaff.FullName : null,
                    AssignedStaffPhoneNumber = x.AssignedStaff != null ? x.AssignedStaff.PhoneNumber : null,

                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AddressDetail = x.AddressDetail,
                    Priority = x.Priority,
                    Status = x.Status,
                    UserRemark = x.UserRemark,
                    StaffRemark = x.StaffRemark,

                    CreatedAt = x.CreatedAt,
                    AcceptedAt = x.AcceptedAt,
                    PreparingAt = x.PreparingAt,
                    DeliveringAt = x.DeliveringAt,
                    CompletedAt = x.CompletedAt,
                    CancelledAt = x.CancelledAt,
                    UpdatedAt = x.UpdatedAt,

                    Items = x.Items.Select(item => new SosRequestItemDto
                    {
                        Id = item.Id,
                        ReliefItemId = item.ReliefItemId,
                        ReliefItemName = item.ReliefItem != null ? item.ReliefItem.Name : string.Empty,
                        Quantity = item.Quantity,
                        Unit = item.Unit
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (request == null)
            {
                return NotFound(new { message = "ไม่พบคำขอความช่วยเหลือ" });
            }

            return Ok(request);
        }


        // PUT: api/sos-requests/{id}/assign
        // PUT: api/sos-requests/{id}/assign
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignSosRequest(
            string id,
            [FromBody] AssignSosRequestDto dto)
        {
            var request = await _context.SosRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบคำขอความช่วยเหลือ"
                });
            }

            if (request.Status != SosRequestStatuses.Pending)
            {
                return BadRequest(new
                {
                    message = "คำขอนี้ถูกรับเรื่องหรือดำเนินการไปแล้ว"
                });
            }

            if (!IsValidPriority(dto.Priority))
            {
                return BadRequest(new
                {
                    message = "ระดับความเร่งด่วนไม่ถูกต้อง"
                });
            }

            var currentUserId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            /*
             * Staff กดรับงานด้วยตัวเอง
             */
            if (User.IsInRole("Staff"))
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสเจ้าหน้าที่จาก Token"
                    });
                }

                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(x =>
                        x.Id == currentUserId &&
                        x.IsActive
                    );

                if (staff == null)
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบเจ้าหน้าที่ หรือบัญชีถูกระงับ"
                    });
                }

                var center = await _context.Centers
                    .FirstOrDefaultAsync(x =>
                        x.Id == staff.CenterId &&
                        x.IsActive
                    );

                if (center == null)
                {
                    return BadRequest(new
                    {
                        message = "ไม่พบศูนย์ของเจ้าหน้าที่ หรือศูนย์ถูกปิดใช้งาน"
                    });
                }

                request.CenterId = staff.CenterId;
                request.AssignedStaffId = staff.Id;
            }
            /*
             * Admin เลือกศูนย์และ Staff ได้
             */
            else if (User.IsInRole("Admin"))
            {
                var center = await _context.Centers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CenterId &&
                        x.IsActive
                    );

                if (center == null)
                {
                    return BadRequest(new
                    {
                        message = "ไม่พบศูนย์ หรือศูนย์ถูกปิดใช้งาน"
                    });
                }

                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.StaffId &&
                        x.CenterId == dto.CenterId &&
                        x.IsActive
                    );

                if (staff == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "ไม่พบเจ้าหน้าที่ในศูนย์ที่เลือก หรือเจ้าหน้าที่ถูกระงับ"
                    });
                }

                request.CenterId = center.Id;
                request.AssignedStaffId = staff.Id;
            }
            else
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "คุณไม่มีสิทธิ์รับคำขอนี้"
                    }
                );
            }

            var now = DateTime.Now;

            request.Priority = dto.Priority;
            request.StaffRemark = dto.StaffRemark?.Trim();
            request.Status = SosRequestStatuses.Accepted;
            request.AcceptedAt = now;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "รับคำขอความช่วยเหลือสำเร็จ",
                data = new
                {
                    request.Id,
                    request.CenterId,
                    request.AssignedStaffId,
                    request.Priority,
                    request.Status,
                    request.AcceptedAt
                }
            });
        }

        // PUT: api/sos-requests/{id}/status
        // PUT: api/sos-requests/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateSosStatus(
            string id,
            [FromBody] UpdateSosStatusDto dto)
        {
            var request = await _context.SosRequests
                .Include(x => x.Items)
                    .ThenInclude(x => x.ReliefItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบคำขอความช่วยเหลือ"
                });
            }

            if (!IsValidStatus(dto.Status))
            {
                return BadRequest(new
                {
                    message = "สถานะไม่ถูกต้อง"
                });
            }

            if (!CanChangeStatus(request.Status, dto.Status))
            {
                return BadRequest(new
                {
                    message =
                        $"ไม่สามารถเปลี่ยนสถานะจาก {request.Status} เป็น {dto.Status} ได้"
                });
            }

            var currentStaffId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            /*
             * Staff ต้องดำเนินการได้เฉพาะ SOS
             * ที่อยู่ในศูนย์ของตัวเองและถูกมอบหมายให้ตัวเอง
             */
            if (User.IsInRole("Staff"))
            {
                if (string.IsNullOrWhiteSpace(currentStaffId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสเจ้าหน้าที่จาก Token"
                    });
                }

                var staff = await _context.Staffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == currentStaffId &&
                        x.IsActive
                    );

                if (staff == null)
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบเจ้าหน้าที่ หรือบัญชีถูกระงับ"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.CenterId))
                {
                    return BadRequest(new
                    {
                        message = "คำขอนี้ยังไม่ได้กำหนดศูนย์ช่วยเหลือ"
                    });
                }

                if (staff.CenterId != request.CenterId)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message =
                                "คุณไม่มีสิทธิ์ดำเนินการคำขอของศูนย์อื่น"
                        }
                    );
                }

                if (request.AssignedStaffId != currentStaffId)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message =
                                "คำขอนี้ไม่ได้ถูกมอบหมายให้เจ้าหน้าที่คนนี้"
                        }
                    );
                }
            }

            /*
             * ตัดสต็อกเฉพาะตอนเปลี่ยน
             * Preparing -> Delivering
             */
            if (dto.Status == SosRequestStatuses.Delivering)
            {
                if (string.IsNullOrWhiteSpace(request.CenterId))
                {
                    return BadRequest(new
                    {
                        message = "คำขอนี้ยังไม่ได้กำหนดศูนย์ช่วยเหลือ"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.AssignedStaffId))
                {
                    return BadRequest(new
                    {
                        message = "คำขอนี้ยังไม่ได้มอบหมายเจ้าหน้าที่"
                    });
                }

                if (request.Items.Count == 0)
                {
                    return BadRequest(new
                    {
                        message = "คำขอนี้ไม่มีรายการสิ่งของ"
                    });
                }

                /*
                 * รวมจำนวนกรณีมี ReliefItem ซ้ำ
                 * เพื่อป้องกันการตรวจสต็อกผิดพลาด
                 */
                var requestedItems = request.Items
                    .GroupBy(x => x.ReliefItemId)
                    .Select(group => new
                    {
                        ReliefItemId = group.Key,
                        Quantity = group.Sum(x => x.Quantity),
                        ReliefItemName = group
                            .Select(x => x.ReliefItem != null
                                ? x.ReliefItem.Name
                                : null)
                            .FirstOrDefault(x =>
                                !string.IsNullOrWhiteSpace(x))
                            ?? group.Key
                    })
                    .ToList();

                var reliefItemIds = requestedItems
                    .Select(x => x.ReliefItemId)
                    .ToList();

                await using var transaction =
                    await _context.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable
                    );

                try
                {
                    /*
                     * โหลด Inventory ภายใน Transaction
                     * เพื่อลดโอกาสที่ Staff หลายคนตัดสต็อกพร้อมกัน
                     */
                    var inventories = await _context.CenterInventories
                        .Where(x =>
                            x.CenterId == request.CenterId &&
                            reliefItemIds.Contains(x.ReliefItemId)
                        )
                        .ToListAsync();

                    var insufficientItems = requestedItems
                        .Select(item =>
                        {
                            var inventory = inventories
                                .FirstOrDefault(x =>
                                    x.ReliefItemId ==
                                    item.ReliefItemId
                                );

                            return new
                            {
                                item.ReliefItemId,
                                item.ReliefItemName,
                                RequestedQuantity =
                                    item.Quantity,
                                AvailableQuantity =
                                    inventory?.Quantity ?? 0
                            };
                        })
                        .Where(x =>
                            x.AvailableQuantity <
                            x.RequestedQuantity
                        )
                        .ToList();

                    if (insufficientItems.Count > 0)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message =
                                "สิ่งของในคลังไม่เพียงพอ",

                            items = insufficientItems
                        });
                    }

                    var nextTransactionId =
                        await GenerateNextInventoryTransactionIdAsync();

                    var now = DateTime.Now;

                    foreach (var requestedItem in requestedItems)
                    {
                        var inventory = inventories.First(x =>
                            x.ReliefItemId ==
                            requestedItem.ReliefItemId
                        );

                        inventory.Quantity -=
                            requestedItem.Quantity;

                        inventory.UpdatedAt = now;

                        _context.InventoryTransactions.Add(
                            new InventoryTransaction
                            {
                                Id = nextTransactionId,

                                CenterInventoryId =
                                    inventory.Id,

                                TransactionType =
                                    "SOSOut",

                                Quantity =
                                    requestedItem.Quantity,

                                BalanceAfter =
                                    inventory.Quantity,

                                ReferenceType =
                                    "SOS",

                                ReferenceId =
                                    request.Id,

                                Note =
                                    $"จ่ายสิ่งของสำหรับ SOS เลขที่ {request.Id}",

                                StaffId =
                                    request.AssignedStaffId,

                                CreatedAt =
                                    now
                            }
                        );

                        nextTransactionId =
                            IncrementTenDigitId(
                                nextTransactionId
                            );
                    }

                    request.Status =
                        SosRequestStatuses.Delivering;

                    request.StaffRemark =
                        dto.StaffRemark?.Trim();

                    request.DeliveringAt = now;
                    request.UpdatedAt = now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message =
                            "อัปเดตเป็นกำลังจัดส่งและตัดสต็อกสำเร็จ",

                        data = new
                        {
                            request.Id,
                            request.Status,
                            request.DeliveringAt,
                            request.UpdatedAt
                        }
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            /*
             * สถานะอื่นไม่ต้องแก้ Inventory
             */
            var updatedAt = DateTime.Now;

            request.Status = dto.Status;
            request.StaffRemark =
                dto.StaffRemark?.Trim();

            request.UpdatedAt = updatedAt;

            switch (dto.Status)
            {
                case SosRequestStatuses.Accepted:
                    request.AcceptedAt ??= updatedAt;
                    break;

                case SosRequestStatuses.Preparing:
                    request.PreparingAt = updatedAt;
                    break;

                case SosRequestStatuses.Completed:
                    request.CompletedAt = updatedAt;
                    break;

                case SosRequestStatuses.Cancelled:
                case SosRequestStatuses.Rejected:
                    request.CancelledAt = updatedAt;
                    break;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "อัปเดตสถานะคำขอสำเร็จ",

                data = new
                {
                    request.Id,
                    request.Status,
                    request.UpdatedAt
                }
            });
        }

        // PUT: api/sos-requests/{id}/cancel
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelMySosRequest(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var request = await _context.SosRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId
                );

            if (request == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบคำขอความช่วยเหลือ"
                });
            }

            if (request.Status != SosRequestStatuses.Pending)
            {
                return BadRequest(new
                {
                    message = "ยกเลิกได้เฉพาะคำขอที่ยังไม่มีเจ้าหน้าที่รับเรื่อง"
                });
            }

            request.Status = SosRequestStatuses.Cancelled;
            request.CancelledAt = DateTime.Now;
            request.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ยกเลิกคำขอความช่วยเหลือสำเร็จ"
            });
        }

        private async Task<string> GenerateNextSosRequestIdAsync()
        {
            var lastId = await _context.SosRequests
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastId))
            {
                return "0000000001";
            }

            if (!int.TryParse(lastId, out var lastNumber))
            {
                throw new InvalidOperationException(
                    $"รหัส SOS ล่าสุดไม่ถูกต้อง: {lastId}"
                );
            }

            var nextNumber = lastNumber + 1;

            if (nextNumber > 99999999)
            {
                throw new InvalidOperationException(
                    "รหัส SOS เต็มแล้ว"
                );
            }

            return nextNumber.ToString("D10");
        }

        private async Task<int> GetNextSosRequestItemNumberAsync()
        {
            var lastId = await _context.SosRequestItems
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastId))
            {
                return 1;
            }

            if (!int.TryParse(lastId, out var lastNumber))
            {
                throw new InvalidOperationException(
                    $"รหัสรายการ SOS ล่าสุดไม่ถูกต้อง: {lastId}"
                );
            }

            return lastNumber + 1;
        }

        private static bool IsValidPriority(string priority)
        {
            return priority == SosPriorities.Normal ||
                   priority == SosPriorities.Urgent ||
                   priority == SosPriorities.Critical;
        }

        private static bool IsValidStatus(string status)
        {
            return status == SosRequestStatuses.Pending ||
                   status == SosRequestStatuses.Accepted ||
                   status == SosRequestStatuses.Preparing ||
                   status == SosRequestStatuses.Delivering ||
                   status == SosRequestStatuses.Completed ||
                   status == SosRequestStatuses.Rejected ||
                   status == SosRequestStatuses.Cancelled;
        }

        private static bool CanChangeStatus(
            string currentStatus,
            string newStatus)
        {
            return currentStatus switch
            {
                SosRequestStatuses.Pending =>
                    newStatus == SosRequestStatuses.Accepted ||
                    newStatus == SosRequestStatuses.Rejected ||
                    newStatus == SosRequestStatuses.Cancelled,

                SosRequestStatuses.Accepted =>
                    newStatus == SosRequestStatuses.Preparing ||
                    newStatus == SosRequestStatuses.Cancelled,

                SosRequestStatuses.Preparing =>
                    newStatus == SosRequestStatuses.Delivering ||
                    newStatus == SosRequestStatuses.Cancelled,

                SosRequestStatuses.Delivering =>
                    newStatus == SosRequestStatuses.Completed,

                _ => false
            };
        }
        // GET: api/sos-requests/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSosStatistics(
            [FromQuery] string? centerId)
        {
            var query = _context.SosRequests
                .AsNoTracking()
                .AsQueryable();

            if (User.IsInRole("Staff"))
            {
                var staffCenterId = User.FindFirstValue("CenterId");

                if (string.IsNullOrWhiteSpace(staffCenterId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสศูนย์จาก Token"
                    });
                }

                query = query.Where(x => x.CenterId == staffCenterId);
            }
            else if (!string.IsNullOrWhiteSpace(centerId))
            {
                query = query.Where(x => x.CenterId == centerId);
            }

            var statistics = new SosStatisticsDto
            {
                Total = await query.CountAsync(),

                Pending = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Pending
                ),

                Accepted = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Accepted
                ),

                Preparing = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Preparing
                ),

                Delivering = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Delivering
                ),

                Completed = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Completed
                ),

                Rejected = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Rejected
                ),

                Cancelled = await query.CountAsync(
                    x => x.Status == SosRequestStatuses.Cancelled
                )
            };

            return Ok(statistics);
        }
        // GET: api/sos-requests/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPendingSosRequests()
        {
            var query = _context.SosRequests
                .AsNoTracking()
                .Where(x => x.Status == SosRequestStatuses.Pending)
                .AsQueryable();

            if (User.IsInRole("Staff"))
            {
                var centerId = User.FindFirstValue("CenterId");

                if (string.IsNullOrWhiteSpace(centerId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสศูนย์จาก Token"
                    });
                }

                query = query.Where(x => x.CenterId == centerId);
            }

            var requests = await query
                .OrderByDescending(x =>
                    x.Priority == SosPriorities.Critical
                )
                .ThenByDescending(x =>
                    x.Priority == SosPriorities.Urgent
                )
                .ThenBy(x => x.CreatedAt)
                .Select(x => new SosRequestListDto
                {
                    Id = x.Id,
                    UserId = x.UserId,

                    UserFullName = x.User != null
                        ? x.User.FullName
                        : string.Empty,

                    UserPhoneNumber = x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,

                    CenterId = x.CenterId,

                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : null,

                    AssignedStaffId = x.AssignedStaffId,

                    AssignedStaffName = x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,

                    AddressDetail = x.AddressDetail,
                    Priority = x.Priority,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }
        // GET: api/sos-requests/staff/me
        [HttpGet("staff/me")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyAssignedSosRequests()
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(staffId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบรหัสเจ้าหน้าที่จาก Token"
                });
            }

            var staffExists = await _context.Staffs
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == staffId &&
                    x.IsActive
                );

            if (!staffExists)
            {
                return Unauthorized(new
                {
                    message = "ไม่พบเจ้าหน้าที่ หรือบัญชีถูกระงับ"
                });
            }

            var requests = await _context.SosRequests
                .AsNoTracking()
                .Where(x => x.AssignedStaffId == staffId)
                .OrderBy(x =>
                    x.Status == SosRequestStatuses.Delivering ? 0 :
                    x.Status == SosRequestStatuses.Preparing ? 1 :
                    x.Status == SosRequestStatuses.Accepted ? 2 :
                    x.Status == SosRequestStatuses.Completed ? 3 : 4
                )
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new SosRequestListDto
                {
                    Id = x.Id,
                    UserId = x.UserId,

                    UserFullName = x.User != null
                        ? x.User.FullName
                        : string.Empty,

                    UserPhoneNumber = x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,

                    CenterId = x.CenterId,

                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : null,

                    AssignedStaffId = x.AssignedStaffId,

                    AssignedStaffName = x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,

                    AddressDetail = x.AddressDetail,
                    Priority = x.Priority,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }
        // GET: api/sos-requests/center/001
        [HttpGet("center/{centerId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSosRequestsByCenter(
            string centerId)
        {
            var normalizedCenterId = centerId.Trim();

            var centerExists = await _context.Centers
                .AsNoTracking()
                .AnyAsync(x => x.Id == normalizedCenterId);

            if (!centerExists)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์ช่วยเหลือ"
                });
            }

            /*
             * ถ้าเป็น Staff ต้องดูได้เฉพาะศูนย์ของตัวเอง
             * Admin สามารถดูได้ทุกศูนย์
             */
            if (User.IsInRole("Staff"))
            {
                var staffCenterId = User.FindFirstValue("CenterId");

                if (string.IsNullOrWhiteSpace(staffCenterId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสศูนย์จาก Token"
                    });
                }

                if (staffCenterId != normalizedCenterId)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message = "คุณไม่มีสิทธิ์ดูข้อมูลของศูนย์อื่น"
                        }
                    );
                }
            }

            var requests = await _context.SosRequests
                .AsNoTracking()
                .Where(x => x.CenterId == normalizedCenterId)
                .OrderBy(x =>
                    x.Status == SosRequestStatuses.Delivering ? 0 :
                    x.Status == SosRequestStatuses.Preparing ? 1 :
                    x.Status == SosRequestStatuses.Accepted ? 2 :
                    x.Status == SosRequestStatuses.Pending ? 3 :
                    x.Status == SosRequestStatuses.Completed ? 4 : 5
                )
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new SosRequestListDto
                {
                    Id = x.Id,
                    UserId = x.UserId,

                    UserFullName = x.User != null
                        ? x.User.FullName
                        : string.Empty,

                    UserPhoneNumber = x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,

                    CenterId = x.CenterId,

                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : null,

                    AssignedStaffId = x.AssignedStaffId,

                    AssignedStaffName = x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,
                    AddressDetail = x.AddressDetail,
                    Priority = x.Priority,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }
        private async Task<string>
    GenerateNextInventoryTransactionIdAsync()
        {
            var lastId = await _context
                .InventoryTransactions
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lastId))
            {
                return "0000000001";
            }

            if (!long.TryParse(
                lastId,
                out var lastNumber))
            {
                throw new InvalidOperationException(
                    $"รหัส InventoryTransaction ล่าสุดไม่ถูกต้อง: {lastId}"
                );
            }

            if (lastNumber >= 9999999999)
            {
                throw new InvalidOperationException(
                    "รหัส InventoryTransaction เต็มแล้ว"
                );
            }

            return (lastNumber + 1)
                .ToString("D10");
        }

        private static string IncrementTenDigitId(
            string currentId)
        {
            if (!long.TryParse(
                currentId,
                out var currentNumber))
            {
                throw new InvalidOperationException(
                    $"รูปแบบรหัสไม่ถูกต้อง: {currentId}"
                );
            }

            if (currentNumber >= 9999999999)
            {
                throw new InvalidOperationException(
                    "รหัส InventoryTransaction เต็มแล้ว"
                );
            }

            return (currentNumber + 1)
                .ToString("D10");
        }
    }
}