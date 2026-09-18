using FloodRelief.Services.Common;
using FloodRelief.Services.Notification;
using FloodRelief.Helpers;
using System.Data;
using System.Security.Claims;
using FloodRelief.Constants;
using FloodRelief.Data;
using FloodRelief.DTOs.Sos;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services
{
    public class SosRequestsService : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUser;
        private readonly NotificationRealtimeService _notificationRealtime;

        public SosRequestsService(
            AppDbContext context,
            CurrentUserService currentUser,
            NotificationRealtimeService notificationRealtime)
        {
            _context = context;
            _currentUser = currentUser;
            _notificationRealtime = notificationRealtime;
        }
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = _currentUser.UserId;

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

                    x.ReceiveMethod,

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
        public async Task<IActionResult> CreateSosRequest(
           CreateSosRequestDto dto)
        {
            var userId = _currentUser.UserId;

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

            var requestLimitErrors = dto.Items
                .Select(dtoItem =>
                {
                    var reliefItem = reliefItems.First(x => x.Id == dtoItem.ReliefItemId);

                    return new
                    {
                        Item = reliefItem,
                        RequestedQuantity = dtoItem.Quantity,
                        ExceedsLimit =
                            reliefItem.MaximumRequestQuantity > 0 &&
                            dtoItem.Quantity > reliefItem.MaximumRequestQuantity
                    };
                })
                .Where(x => x.ExceedsLimit)
                .ToList();

            if (requestLimitErrors.Count > 0)
            {
                return BadRequest(new
                {
                    message = "จำนวนสิ่งของที่ขอเกินจำนวนสูงสุดที่กำหนด",
                    items = requestLimitErrors.Select(x => new
                    {
                        reliefItemId = x.Item.Id,
                        name = x.Item.Name,
                        unit = x.Item.Unit,
                        requestedQuantity = x.RequestedQuantity,
                        maximumRequestQuantity = x.Item.MaximumRequestQuantity
                    })
                });
            }

            var receiveMethod = string.Equals(
                dto.ReceiveMethod,
                "Pickup",
                StringComparison.OrdinalIgnoreCase
            )
                ? "Pickup"
                : "Delivery";

            FloodRelief.Models.Center? pickupCenter = null;

            if (receiveMethod == "Pickup")
            {
                pickupCenter = await _context.Centers
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (pickupCenter == null)
                {
                    return BadRequest(new
                    {
                        message = "ไม่พบศูนย์ช่วยเหลือที่เปิดใช้งานสำหรับรับสิ่งของ"
                    });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.AddressDetail))
                {
                    return BadRequest(new
                    {
                        message = "กรุณาระบุตำแหน่งสำหรับจัดส่งสิ่งของ"
                    });
                }

                if (dto.Latitude == 0 && dto.Longitude == 0)
                {
                    return BadRequest(new
                    {
                        message = "กรุณาปักหมุดตำแหน่งสำหรับจัดส่งสิ่งของ"
                    });
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );
            try
            {
                var requestId = await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.SosRequests,
                    x => x.Id,
                    "SosRequest");

                var request = new SosRequest
                {
                    Id = requestId,
                    UserId = userId,
                    RequestType = "Relief",
                    ReceiveMethod = receiveMethod,
                    CenterId = pickupCenter?.Id,
                    AssignedStaffId = null,
                    Latitude = receiveMethod == "Pickup"
                        ? pickupCenter!.Latitude
                        : dto.Latitude,
                    Longitude = receiveMethod == "Pickup"
                        ? pickupCenter!.Longitude
                        : dto.Longitude,
                    AddressDetail = receiveMethod == "Pickup"
                        ? pickupCenter!.Address
                        : dto.AddressDetail!.Trim(),
                    UserRemark = dto.UserRemark?.Trim(),
                    Priority = SosPriorities.Normal,
                    Status = SosRequestStatuses.Pending,
                    CreatedAt = DateTime.Now
                };

                var nextItemId =
                    await PrimaryKeyHelper.GenerateNextIdAsync(
                        _context.SosRequestItems,
                        x => x.Id,
                        "SosRequestItem",
                        digits: 8);

                foreach (var dtoItem in dto.Items)
                {
                    var reliefItem = reliefItems
                        .First(x => x.Id == dtoItem.ReliefItemId);

                    request.Items.Add(new SosRequestItem
                    {
                        Id = nextItemId,
                        SosRequestId = request.Id,
                        ReliefItemId = reliefItem.Id,
                        Quantity = dtoItem.Quantity,
                        Unit = reliefItem.Unit,
                        CreatedAt = DateTime.Now
                    });

                    nextItemId = PrimaryKeyHelper.IncrementId(
                        nextItemId,
                        "SosRequestItem",
                        digits: 8);
                }

                _context.SosRequests.Add(request);
                await _notificationRealtime.SaveChangesAsync();

                await AddNewRequestNotificationsToAllStaffAsync(
                    request,
                    type: "StaffNewRelief",
                    title: "มีคำขอรับสิ่งของใหม่",
                    message: $"คำขอรับสิ่งของ #{request.Id} รอเจ้าหน้าที่รับงาน ที่อยู่: {request.AddressDetail}"
                );

                await _notificationRealtime.SaveChangesAsync();
                await transaction.CommitAsync();
                await _notificationRealtime.FlushAsync();

                return CreatedAtAction(
                    nameof(GetSosRequestById),
                    new { id = request.Id },
                    new
                    {
                        message = "ส่งคำขอความช่วยเหลือสำเร็จ",
                        sosRequestId = request.Id,
                        receiveMethod = request.ReceiveMethod,
                        status = request.Status,
                        createdAt = request.CreatedAt
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                _notificationRealtime.DiscardPending();
                throw;
            }
        }


        public async Task<IActionResult> CreateEmergencySosRequest(
            CreateEmergencySosRequestDto dto)
        {
            var userId = _currentUser.UserId;

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

            if (!EmergencyTypes.All.Contains(dto.EmergencyType))
            {
                return BadRequest(new
                {
                    message = "ประเภทเหตุฉุกเฉินไม่ถูกต้อง"
                });
            }

            if (!SosSeverities.All.Contains(dto.Severity))
            {
                return BadRequest(new
                {
                    message = "ระดับความรุนแรงของผู้ประสบภัยไม่ถูกต้อง"
                });
            }

            if (dto.Latitude == 0 && dto.Longitude == 0)
            {
                return BadRequest(new
                {
                    message = "กรุณาปักหมุดตำแหน่งเหตุฉุกเฉิน"
                });
            }

            var accountedVictimTotal =
                dto.ChildCount +
                dto.ElderlyCount +
                dto.DisabledCount +
                dto.PatientCount +
                dto.DeathCount;

            if (accountedVictimTotal > dto.VictimCount)
            {
                return BadRequest(new
                {
                    message = "จำนวนเด็ก ผู้สูงอายุ ผู้พิการ ผู้ป่วย และผู้เสียชีวิตรวมกันต้องไม่เกินจำนวนผู้ประสบภัยทั้งหมด"
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            try
            {
                var requestId = await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.SosRequests,
                    x => x.Id,
                    "SosRequest");

                var request = new SosRequest
                {
                    Id = requestId,
                    UserId = userId,
                    RequestType = "Emergency",
                    ReceiveMethod = null,
                    CenterId = null,
                    AssignedStaffId = null,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    AddressDetail = dto.AddressDetail.Trim(),
                    EmergencyType = dto.EmergencyType,
                    VictimCount = dto.VictimCount,
                    ChildCount = dto.ChildCount,
                    ElderlyCount = dto.ElderlyCount,
                    DisabledCount = dto.DisabledCount,
                    PatientCount = dto.PatientCount,
                    DeathCount = dto.DeathCount,
                    Severity = dto.Severity,
                    WaterLevel = dto.WaterLevel,
                    EmergencyDetail = dto.EmergencyDetail.Trim(),
                    UserRemark = dto.EmergencyDetail.Trim(),
                    Priority = SosPriorities.Critical,
                    Status = SosRequestStatuses.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.SosRequests.Add(request);

                var nextNotificationId =
                    await PrimaryKeyHelper.GenerateNextIdAsync(
                        _context.Notifications,
                        x => x.Id,
                        "Notification");

                _context.Notifications.Add(
                    new FloodRelief.Models.Notification
                    {
                        Id = nextNotificationId,
                        UserId = request.UserId,
                        Type = "SosCreated",
                        Title = "ระบบได้รับ SOS ของคุณแล้ว",
                        Message =
                            $"ระบบได้รับเคส SOS #{request.Id} และกำลังส่งข้อมูลไปยังเจ้าหน้าที่ กรุณาอยู่ในจุดที่ปลอดภัย",
                        ReferenceType = "SosRequest",
                        ReferenceId = request.Id,
                        IsRead = false,
                        CreatedAt = request.CreatedAt
                    }
                );

                await _notificationRealtime.SaveChangesAsync();

                await AddNewRequestNotificationsToAllStaffAsync(
                    request,
                    type: "StaffNewSos",
                    title: "มี SOS ฉุกเฉินใหม่",
                    message: $"SOS #{request.Id} ต้องการความช่วยเหลือด่วน ที่อยู่: {request.AddressDetail}"
                );

                await _notificationRealtime.SaveChangesAsync();
                await transaction.CommitAsync();
                await _notificationRealtime.FlushAsync();

                return CreatedAtAction(
                    nameof(GetSosRequestById),
                    new { id = request.Id },
                    new
                    {
                        message = "ส่ง SOS ฉุกเฉินสำเร็จ",
                        sosRequestId = request.Id,
                        requestType = request.RequestType,
                        emergencyType = request.EmergencyType,
                        deathCount = request.DeathCount,
                        severity = request.Severity,
                        priority = request.Priority,
                        status = request.Status,
                        createdAt = request.CreatedAt
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                _notificationRealtime.DiscardPending();
                throw;
            }
        }

        private static string DetermineEmergencyPriority(
            CreateEmergencySosRequestDto dto)
        {
            if (
                dto.EmergencyType == EmergencyTypes.Medical ||
                dto.EmergencyType == EmergencyTypes.Injured ||
                dto.EmergencyType == EmergencyTypes.RoofTrapped ||
                dto.PatientCount > 0 ||
                dto.DisabledCount > 0 ||
                dto.WaterLevel >= 1.5m)
            {
                return SosPriorities.Critical;
            }

            if (
                dto.EmergencyType == EmergencyTypes.Trapped ||
                dto.EmergencyType == EmergencyTypes.RapidFlood ||
                dto.EmergencyType == EmergencyTypes.Evacuation ||
                dto.ElderlyCount > 0 ||
                dto.ChildCount > 0 ||
                dto.VictimCount >= 5 ||
                dto.WaterLevel >= 0.5m)
            {
                return SosPriorities.Urgent;
            }

            return SosPriorities.Normal;
        }

        // GET: api/sos-requests/my
        // GET: api/sos-requests/my
        public async Task<IActionResult> GetMySosRequests(
            DateTime? startDate,
            DateTime? endDate,
            string? status)
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้จาก Token"
                });
            }
          var query = _context.SosRequests
                .AsNoTracking()
                .Where(x => x.UserId == userId);
            // วันที่เริ่มต้น
            if (startDate.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt >= startDate.Value.Date
                );
            }
            // วันที่สิ้นสุด
            if (endDate.HasValue)
            {
                var endDateNext =
                    endDate.Value.Date.AddDays(1);


                query = query.Where(x =>
                    x.CreatedAt < endDateNext
                );
            }
            // Status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x =>
                    x.Status == status
                );
            }
            var requests = await query

                .OrderByDescending(x => x.CreatedAt)

                .Select(x => new SosRequestListDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserFullName =
                        x.User != null
                        ? x.User.FullName
                        : string.Empty,
                    UserPhoneNumber =
                        x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,
                    CenterId = x.CenterId,
                    CenterName =
                        x.Center != null
                        ? x.Center.CenterName
                        : null,
                    AssignedStaffId =
                        x.AssignedStaffId,
                    AssignedStaffName =
                        x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AddressDetail =
                        x.AddressDetail,
                    RequestType = x.RequestType,
                    ReceiveMethod = x.ReceiveMethod,
                    EmergencyType = x.EmergencyType,
                    VictimCount = x.VictimCount,
                    ChildCount = x.ChildCount,
                    ElderlyCount = x.ElderlyCount,
                    DisabledCount = x.DisabledCount,
                    PatientCount = x.PatientCount,
                    DeathCount = x.DeathCount,
                    Severity = x.Severity,
                    WaterLevel = x.WaterLevel,
                    EmergencyDetail = x.EmergencyDetail,
                    Items = x.Items.Select(i => new SosRequestItemDto
                    {
                        Id = i.Id,
                        ReliefItemId = i.ReliefItemId,
                        ReliefItemName = i.ReliefItem != null ? i.ReliefItem.Name : "ไม่ระบุ",
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    }).ToList(),
                    Priority =
                        x.Priority,
                    Status =
                        x.Status,
                    CreatedAt =
                        x.CreatedAt,
                    UpdatedAt =
                        x.UpdatedAt

                })
                .ToListAsync();
            return Ok(requests);
        }

        // GET: api/sos-requests
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
        x.RequestType,
        x.ReceiveMethod,
        x.EmergencyType,
        x.VictimCount,
        x.ChildCount,
        x.ElderlyCount,
        x.DisabledCount,
        x.PatientCount,
        x.DeathCount,
        x.Severity,
        x.WaterLevel,
        x.EmergencyDetail,
        x.Priority,
        x.Status,
        x.CreatedAt
    })
    .ToListAsync();

            var sortedRequests = requests
                .OrderBy(x =>
                    GetPriorityOrder(x.Priority)
                )
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(sortedRequests);
        }

        // GET: api/sos-requests/{id}
        public async Task<IActionResult> GetSosRequestById(string id)
        {
            var userId = _currentUser.UserId;


            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้"
                });
            }


            var query = _context.SosRequests
                .AsNoTracking()
                .Where(x => x.Id == id);

            if (_currentUser.IsInRole("User"))
            {
                query = query.Where(x => x.UserId == userId);
            }
            else if (_currentUser.IsInRole("Staff"))
            {
                query = query.Where(x =>
                    x.Status == SosRequestStatuses.Pending ||
                    x.AssignedStaffId == userId
                );
            }
            else if (!_currentUser.IsInRole("Admin"))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = "คุณไม่มีสิทธิ์ดูคำขอนี้" }
                );
            }

            var request = await query
                .Select(x => new SosRequestDetailDto
                {

                    Id = x.Id,

                    UserId = x.UserId,


                    UserFullName =
                        x.User != null
                        ? x.User.FullName
                        : "",


                    UserPhoneNumber =
                        x.User != null
                        ? x.User.PhoneNumber
                        : "",


                    UserEmail =
                        x.User != null
                        ? x.User.Email
                        : "",



                    CenterId = x.CenterId,


                    CenterName =
                        x.Center != null
                        ? x.Center.CenterName
                        : null,


                    CenterPhoneNumber =
                        x.Center != null
                        ? x.Center.PhoneNumber
                        : null,



                    AssignedStaffId =
                        x.AssignedStaffId,


                    AssignedStaffName =
                        x.AssignedStaff != null
                        ? x.AssignedStaff.FullName
                        : null,


                    AssignedStaffPhoneNumber =
                        x.AssignedStaff != null
                        ? x.AssignedStaff.PhoneNumber
                        : null,



                    Latitude = x.Latitude,

                    Longitude = x.Longitude,


                    AddressDetail = x.AddressDetail,

                    RequestType = x.RequestType,
                    ReceiveMethod = x.ReceiveMethod,
                    EmergencyType = x.EmergencyType,
                    VictimCount = x.VictimCount,
                    ChildCount = x.ChildCount,
                    ElderlyCount = x.ElderlyCount,
                    DisabledCount = x.DisabledCount,
                    PatientCount = x.PatientCount,
                    DeathCount = x.DeathCount,
                    Severity = x.Severity,
                    WaterLevel = x.WaterLevel,
                    EmergencyDetail = x.EmergencyDetail,

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



                    Items =
                        x.Items.Select(i => new SosRequestItemDto
                        {

                            Id = i.Id,


                            ReliefItemId = i.ReliefItemId,


                            ReliefItemName =
                                i.ReliefItem != null
                                ? i.ReliefItem.Name
                                : "ไม่ระบุ",


                            Quantity = i.Quantity,


                            Unit = i.Unit


                        }).ToList()

                })


                .FirstOrDefaultAsync();



            if (request == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบคำขอ"
                });
            }


            return Ok(request);
        }


        // PUT: api/sos-requests/{id}/assign
        // PUT: api/sos-requests/{id}/assign
        public async Task<IActionResult> AssignSosRequest(
            string id,
            AssignSosRequestDto dto)
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

            var currentUserId = _currentUser.UserId;
            var isPickupRequest = string.Equals(
                request.ReceiveMethod,
                "Pickup",
                StringComparison.OrdinalIgnoreCase
            );

            /*
             * Staff กดรับงานด้วยตัวเอง
             */
            if (_currentUser.IsInRole("Staff"))
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

                if (
                    isPickupRequest &&
                    !string.Equals(
                        staff.CenterId,
                        request.CenterId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message =
                                "คำขอรับเองที่ศูนย์นี้ต้องดำเนินการโดยเจ้าหน้าที่ของศูนย์ที่ระบุในคำขอ"
                        }
                    );
                }

                request.CenterId = staff.CenterId;
                request.AssignedStaffId = staff.Id;
            }
            /*
             * Admin เลือกศูนย์และ Staff ได้
             */
            else if (_currentUser.IsInRole("Admin"))
            {
                if (string.IsNullOrWhiteSpace(dto.CenterId) ||
                    string.IsNullOrWhiteSpace(dto.StaffId))
                {
                    return BadRequest(new
                    {
                        message = "Admin ต้องระบุศูนย์และเจ้าหน้าที่"
                    });
                }

                if (
                    isPickupRequest &&
                    !string.Equals(
                        dto.CenterId,
                        request.CenterId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return BadRequest(new
                    {
                        message =
                            "คำขอรับเองที่ศูนย์นี้ไม่สามารถเปลี่ยนไปใช้ศูนย์อื่นได้"
                    });
                }

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

            request.StaffRemark = dto.StaffRemark?.Trim();
            request.Status = SosRequestStatuses.Accepted;
            request.AcceptedAt = now;
            request.UpdatedAt = now;

            var isEmergencyRequest = string.Equals(
                request.RequestType,
                "Emergency",
                StringComparison.OrdinalIgnoreCase
            );

            var nextNotificationId =
                await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.Notifications,
                    x => x.Id,
                    "Notification");

            _context.Notifications.Add(
                new FloodRelief.Models.Notification
                {
                    Id = nextNotificationId,
                    UserId = request.UserId,
                    Type = isEmergencyRequest
                        ? "SosAccepted"
                        : "ReliefAccepted",
                    Title = isEmergencyRequest
                        ? "เจ้าหน้าที่รับเคส SOS ของคุณแล้ว"
                        : "เจ้าหน้าที่รับคำขอของคุณแล้ว",
                    Message = isEmergencyRequest
                        ? $"เคส SOS #{request.Id} มีเจ้าหน้าที่รับผิดชอบแล้ว กรุณาติดตามสถานะเพื่อรอการเข้าช่วยเหลือ"
                        : $"คำขอรับสิ่งของ #{request.Id} มีเจ้าหน้าที่รับเรื่องแล้ว และกำลังดำเนินการตามคำขอของคุณ",
                    ReferenceType = "SosRequest",
                    ReferenceId = request.Id,
                    IsRead = false,
                    CreatedAt = now
                }
            );

            if (
                _currentUser.IsInRole("Admin") &&
                !string.IsNullOrWhiteSpace(request.AssignedStaffId)
            )
            {
                nextNotificationId =
                    PrimaryKeyHelper.IncrementId(
                        nextNotificationId,
                        "Notification"
                    );

                _context.Notifications.Add(
                    new FloodRelief.Models.Notification
                    {
                        Id = nextNotificationId,
                        StaffId = request.AssignedStaffId,
                        Type = "StaffCaseAssigned",
                        Title = "คุณได้รับมอบหมายเคสใหม่",
                        Message = isEmergencyRequest
                            ? $"Admin มอบหมาย SOS #{request.Id} ให้คุณรับผิดชอบ กรุณาตรวจสอบและดำเนินการช่วยเหลือ"
                            : $"Admin มอบหมายคำขอรับสิ่งของ #{request.Id} ให้คุณรับผิดชอบ กรุณาตรวจสอบรายละเอียดเคส",
                        ReferenceType = "SosRequest",
                        ReferenceId = request.Id,
                        IsRead = false,
                        CreatedAt = now
                    }
                );
            }

            await _notificationRealtime.SaveChangesAsync();

            return Ok(new
            {
                message = "รับคำขอความช่วยเหลือสำเร็จ",
                data = new
                {
                    request.Id,
                    request.CenterId,
                    request.AssignedStaffId,
                    request.Status,
                    request.AcceptedAt
                }
            });
        }

        // PUT: api/sos-requests/{id}/status
        // PUT: api/sos-requests/{id}/status
        public async Task<IActionResult> UpdateSosStatus(
            string id,
            UpdateSosStatusDto dto)
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

            var currentStaffId = _currentUser.UserId;

            /*
             * Staff ต้องดำเนินการได้เฉพาะ SOS
             * ที่อยู่ในศูนย์ของตัวเองและถูกมอบหมายให้ตัวเอง
             */
            if (_currentUser.IsInRole("Staff"))
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

                var isEmergencyRequest = string.Equals(
                    request.RequestType,
                    "Emergency",
                    StringComparison.OrdinalIgnoreCase
                );

                /*
                 * Emergency SOS ไม่มีรายการสิ่งของและไม่ต้องตัด Inventory
                 * เมื่อเข้าสู่ Delivering ให้ตีความว่าเจ้าหน้าที่กำลังเดินทาง
                 */
                if (isEmergencyRequest)
                {
                    var emergencyDeliveringAt = DateTime.Now;

                    request.Status = SosRequestStatuses.Delivering;
                    request.StaffRemark = dto.StaffRemark?.Trim();
                    request.DeliveringAt = emergencyDeliveringAt;
                    request.UpdatedAt = emergencyDeliveringAt;

                    var emergencyNotificationId =
                        await PrimaryKeyHelper.GenerateNextIdAsync(
                            _context.Notifications,
                            x => x.Id,
                            "Notification");

                    _context.Notifications.Add(
                        new FloodRelief.Models.Notification
                        {
                            Id = emergencyNotificationId,
                            UserId = request.UserId,
                            Type = "SosResponderOnTheWay",
                            Title = "เจ้าหน้าที่กำลังเดินทางไปยังตำแหน่งของคุณ",
                            Message =
                                $"เจ้าหน้าที่กำลังเดินทางไปช่วยเหลือเคส SOS #{request.Id} กรุณาอยู่ในจุดที่ปลอดภัยและติดตามสถานะ",
                            ReferenceType = "SosRequest",
                            ReferenceId = request.Id,
                            IsRead = false,
                            CreatedAt = emergencyDeliveringAt
                        }
                    );

                    await _notificationRealtime.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "อัปเดตสถานะเป็นเจ้าหน้าที่กำลังเดินทางสำเร็จ",
                        data = new
                        {
                            request.Id,
                            request.Status,
                            request.DeliveringAt,
                            request.UpdatedAt
                        }
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
                        _notificationRealtime.DiscardPending();

                        return BadRequest(new
                        {
                            message =
                                "สิ่งของในคลังไม่เพียงพอ",

                            items = insufficientItems
                        });
                    }

                    var nextTransactionId =
                        await PrimaryKeyHelper.GenerateNextIdAsync(
                            _context.InventoryTransactions,
                            x => x.Id,
                            "InventoryTransaction");

                    var nextAllocationId =
                        await PrimaryKeyHelper.GenerateNextIdAsync(
                            _context.DonationAllocations,
                            x => x.Id,
                            "DonationAllocation");

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
                            PrimaryKeyHelper.IncrementId(
                                nextTransactionId,
                                "InventoryTransaction"
                            );

                        /*
                         * ผูกสต็อกที่จ่ายออกกับล็อต Donation แบบ FIFO
                         * เฉพาะคำขอรับสิ่งของ (Relief) เท่านั้น
                         *
                         * ถ้าเป็นสต็อกเก่าที่ไม่มี DonationBatch ระบบยังจ่ายได้ตาม
                         * CenterInventory แต่จะไม่อ้างว่าเป็นของผู้บริจาครายใด
                         */
                        {
                            var remainingToTrace =
                                requestedItem.Quantity;

                            var donationBatches = await _context.DonationBatches
                                .Include(x => x.Donation)
                                .Include(x => x.ReliefItem)
                                .Where(x =>
                                    x.CenterId == request.CenterId &&
                                    x.ReliefItemId == requestedItem.ReliefItemId &&
                                    x.RemainingQuantity > 0
                                )
                                .OrderBy(x => x.ReceivedAt)
                                .ThenBy(x => x.Id)
                                .ToListAsync();

                            foreach (var batch in donationBatches)
                            {
                                if (remainingToTrace <= 0)
                                {
                                    break;
                                }

                                var allocatedQuantity = Math.Min(
                                    batch.RemainingQuantity,
                                    remainingToTrace
                                );

                                if (allocatedQuantity <= 0)
                                {
                                    continue;
                                }

                                batch.RemainingQuantity -= allocatedQuantity;
                                remainingToTrace -= allocatedQuantity;

                                _context.DonationAllocations.Add(
                                    new DonationAllocation
                                    {
                                        Id = nextAllocationId,
                                        DonationBatchId = batch.Id,
                                        SosRequestId = request.Id,
                                        ReliefItemId = requestedItem.ReliefItemId,
                                        Quantity = allocatedQuantity,
                                        AllocatedAt = now
                                    }
                                );

                                nextAllocationId =
                                    PrimaryKeyHelper.IncrementId(
                                        nextAllocationId,
                                        "DonationAllocation"
                                    );
                            }
                        }
                    }

                    request.Status =
                        SosRequestStatuses.Delivering;

                    request.StaffRemark =
                        dto.StaffRemark?.Trim();

                    request.DeliveringAt = now;
                    request.UpdatedAt = now;

                    // บันทึก Inventory + Allocation ก่อน เพื่อให้ query trace ได้ใน transaction เดียวกัน
                    await _notificationRealtime.SaveChangesAsync();

                    var nextNotificationId =
                        await PrimaryKeyHelper.GenerateNextIdAsync(
                            _context.Notifications,
                            x => x.Id,
                            "Notification");

                    var isPickupRequest = string.Equals(
                        request.ReceiveMethod,
                        "Pickup",
                        StringComparison.OrdinalIgnoreCase
                    );

                    // แจ้งผู้ขอรับของตามวิธีรับสิ่งของ
                    _context.Notifications.Add(
                        new FloodRelief.Models.Notification
                        {
                            Id = nextNotificationId,
                            UserId = request.UserId,
                            Type = isPickupRequest
                                ? "ReliefReadyForPickup"
                                : "ReliefDelivering",
                            Title = isPickupRequest
                                ? "สิ่งของพร้อมให้รับที่ศูนย์แล้ว"
                                : "สิ่งของกำลังเดินทางไปหาคุณ",
                            Message = isPickupRequest
                                ? $"คำขอรับสิ่งของ #{request.Id} จัดเตรียมเรียบร้อยแล้ว กรุณามารับสิ่งของที่ศูนย์ที่รับผิดชอบ"
                                : $"คำขอรับสิ่งของ #{request.Id} กำลังนำส่งไปยังตำแหน่งที่คุณระบุ กรุณาเตรียมรับสิ่งของ",
                            ReferenceType = "SosRequest",
                            ReferenceId = request.Id,
                            IsRead = false,
                            CreatedAt = now
                        }
                    );

                    nextNotificationId =
                        PrimaryKeyHelper.IncrementId(
                            nextNotificationId,
                            "Notification"
                        );

                    // แจ้งผู้บริจาคเฉพาะล็อตที่ถูก allocate ให้เคสนี้จริง ๆ
                    var inTransitAllocations = await _context.DonationAllocations
                        .Where(x => x.SosRequestId == request.Id)
                        .Include(x => x.DonationBatch)
                            .ThenInclude(x => x.Donation)
                        .Include(x => x.ReliefItem)
                        .ToListAsync();

                    var donorInTransitGroups = inTransitAllocations
                        .Where(x =>
                            x.DonationBatch?.Donation != null &&
                            !string.IsNullOrWhiteSpace(
                                x.DonationBatch.Donation.UserId
                            )
                        )
                        .GroupBy(x => new
                        {
                            DonationId = x.DonationBatch.DonationId,
                            UserId = x.DonationBatch.Donation.UserId
                        })
                        .ToList();

                    foreach (var donorGroup in donorInTransitGroups)
                    {
                        var itemSummaries = donorGroup
                            .GroupBy(x => new
                            {
                                x.ReliefItemId,
                                Name = x.ReliefItem != null
                                    ? x.ReliefItem.Name
                                    : x.ReliefItemId,
                                Unit = x.ReliefItem != null
                                    ? x.ReliefItem.Unit
                                    : string.Empty
                            })
                            .Select(group =>
                                $"{group.Key.Name} {group.Sum(x => x.Quantity)} {group.Key.Unit}".Trim()
                            )
                            .ToList();

                        var visibleItems = string.Join(
                            ", ",
                            itemSummaries.Take(3)
                        );

                        if (itemSummaries.Count > 3)
                        {
                            visibleItems +=
                                $" และอีก {itemSummaries.Count - 3} รายการ";
                        }

                        var donorMessage =
                            $"{visibleItems} จากการบริจาค #{donorGroup.Key.DonationId} กำลังถูกนำไปช่วยเหลือในเคส #{request.Id}";

                        if (donorMessage.Length > 500)
                        {
                            donorMessage =
                                donorMessage[..497] + "...";
                        }

                        _context.Notifications.Add(
                            new FloodRelief.Models.Notification
                            {
                                Id = nextNotificationId,
                                UserId = donorGroup.Key.UserId,
                                Type = "DonationInTransit",
                                Title = "สิ่งของของคุณกำลังถูกส่งต่อ",
                                Message = donorMessage,
                                ReferenceType = "Donation",
                                ReferenceId = donorGroup.Key.DonationId,
                                IsRead = false,
                                CreatedAt = now
                            }
                        );

                        nextNotificationId =
                            PrimaryKeyHelper.IncrementId(
                                nextNotificationId,
                                "Notification"
                            );
                    }

                    await _notificationRealtime.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await _notificationRealtime.FlushAsync();

                    return Ok(new
                    {
                        message = isPickupRequest
                            ? "เตรียมสิ่งของพร้อมให้รับที่ศูนย์และตัดสต็อกสำเร็จ"
                            : "อัปเดตเป็นกำลังจัดส่งและตัดสต็อกสำเร็จ",

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
                    _notificationRealtime.DiscardPending();
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

                    var isEmergencyCompleted = string.Equals(
                        request.RequestType,
                        "Emergency",
                        StringComparison.OrdinalIgnoreCase
                    );

                    var nextCompletedNotificationId =
                        await PrimaryKeyHelper.GenerateNextIdAsync(
                            _context.Notifications,
                            x => x.Id,
                            "Notification");

                    // แจ้งเจ้าของคำขอเมื่อเคสเสร็จสิ้น
                    _context.Notifications.Add(
                        new FloodRelief.Models.Notification
                        {
                            Id = nextCompletedNotificationId,
                            UserId = request.UserId,
                            Type = isEmergencyCompleted
                                ? "SosCompleted"
                                : "ReliefCompleted",
                            Title = isEmergencyCompleted
                                ? "เคส SOS ได้รับการช่วยเหลือเรียบร้อยแล้ว"
                                : string.Equals(
                                    request.ReceiveMethod,
                                    "Pickup",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                    ? "รับสิ่งของเรียบร้อยแล้ว"
                                    : "ส่งมอบสิ่งของเรียบร้อยแล้ว",
                            Message = isEmergencyCompleted
                                ? $"เคส SOS #{request.Id} ถูกปิดหลังจากดำเนินการช่วยเหลือเรียบร้อยแล้ว"
                                : string.Equals(
                                    request.ReceiveMethod,
                                    "Pickup",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                    ? $"คำขอรับสิ่งของ #{request.Id} รับสิ่งของที่ศูนย์เรียบร้อยแล้ว ขอบคุณที่ใช้ระบบ Flood Relief"
                                    : $"คำขอรับสิ่งของ #{request.Id} ถูกส่งมอบเรียบร้อยแล้ว ขอบคุณที่ใช้ระบบ Flood Relief",
                            ReferenceType = "SosRequest",
                            ReferenceId = request.Id,
                            IsRead = false,
                            CreatedAt = updatedAt
                        }
                    );

                    nextCompletedNotificationId =
                        PrimaryKeyHelper.IncrementId(
                            nextCompletedNotificationId,
                            "Notification"
                        );

                    /*
                     * เมื่อช่วยเหลือสำเร็จ ให้แจ้งเฉพาะผู้บริจาคที่ล็อตของเขา
                     * ถูก allocate ให้คำขอนี้จริง ๆ โดยไม่เปิดเผยข้อมูลส่วนตัว
                     * ของผู้ประสบภัย
                     */
                    var completedAllocations = await _context.DonationAllocations
                        .Where(x => x.SosRequestId == request.Id)
                        .Include(x => x.DonationBatch)
                            .ThenInclude(x => x.Donation)
                        .Include(x => x.ReliefItem)
                        .ToListAsync();

                    var donorGroups = completedAllocations
                        .Where(x =>
                            x.DonationBatch?.Donation != null &&
                            !string.IsNullOrWhiteSpace(
                                x.DonationBatch.Donation.UserId
                            )
                        )
                        .GroupBy(x => new
                        {
                            DonationId = x.DonationBatch.DonationId,
                            UserId = x.DonationBatch.Donation.UserId
                        })
                        .ToList();

                    if (donorGroups.Count > 0)
                    {
                        foreach (var donorGroup in donorGroups)
                        {
                            var alreadyNotified =
                                await _context.Notifications.AnyAsync(x =>
                                    x.UserId == donorGroup.Key.UserId &&
                                    x.Type == "DonationDelivered" &&
                                    x.ReferenceType == "Donation" &&
                                    x.ReferenceId == donorGroup.Key.DonationId &&
                                    x.Message.Contains(request.Id)
                                );

                            if (alreadyNotified)
                            {
                                continue;
                            }

                            var itemSummaries = donorGroup
                                .GroupBy(x => new
                                {
                                    x.ReliefItemId,
                                    Name = x.ReliefItem != null
                                        ? x.ReliefItem.Name
                                        : x.ReliefItemId,
                                    Unit = x.ReliefItem != null
                                        ? x.ReliefItem.Unit
                                        : string.Empty
                                })
                                .Select(group =>
                                    $"{group.Key.Name} {group.Sum(x => x.Quantity)} {group.Key.Unit}".Trim()
                                )
                                .ToList();

                            var visibleItems = string.Join(
                                ", ",
                                itemSummaries.Take(3)
                            );

                            if (itemSummaries.Count > 3)
                            {
                                visibleItems +=
                                    $" และอีก {itemSummaries.Count - 3} รายการ";
                            }

                            var notificationMessage =
                                $"{visibleItems} จากการบริจาค #{donorGroup.Key.DonationId} ถูกส่งถึงผู้ประสบภัยในเคสช่วยเหลือ #{request.Id} เรียบร้อยแล้ว";

                            if (notificationMessage.Length > 500)
                            {
                                notificationMessage =
                                    notificationMessage[..497] + "...";
                            }

                            _context.Notifications.Add(
                                new FloodRelief.Models.Notification
                                {
                                    Id = nextCompletedNotificationId,
                                    UserId = donorGroup.Key.UserId,
                                    Type = "DonationDelivered",
                                    Title = "สิ่งของของคุณถูกส่งต่อแล้ว",
                                    Message = notificationMessage,
                                    ReferenceType = "Donation",
                                    ReferenceId = donorGroup.Key.DonationId,
                                    IsRead = false,
                                    CreatedAt = updatedAt
                                }
                            );

                            nextCompletedNotificationId =
                                PrimaryKeyHelper.IncrementId(
                                    nextCompletedNotificationId,
                                    "Notification"
                                );
                        }
                    }

                    break;

                case SosRequestStatuses.Cancelled:
                case SosRequestStatuses.Rejected:
                    request.CancelledAt = updatedAt;

                    if (
                        !string.IsNullOrWhiteSpace(request.AssignedStaffId) &&
                        !(
                            _currentUser.IsInRole("Staff") &&
                            request.AssignedStaffId == _currentUser.UserId
                        )
                    )
                    {
                        var cancelledNotificationId =
                            await PrimaryKeyHelper.GenerateNextIdAsync(
                                _context.Notifications,
                                x => x.Id,
                                "Notification"
                            );

                        _context.Notifications.Add(
                            new FloodRelief.Models.Notification
                            {
                                Id = cancelledNotificationId,
                                StaffId = request.AssignedStaffId,
                                Type = "StaffCaseCancelled",
                                Title = "เคสที่คุณรับผิดชอบถูกยกเลิก",
                                Message =
                                    $"เคส #{request.Id} ถูกยกเลิกหรือปฏิเสธ กรุณาหยุดการดำเนินงานสำหรับเคสนี้",
                                ReferenceType = "SosRequest",
                                ReferenceId = request.Id,
                                IsRead = false,
                                CreatedAt = updatedAt
                            }
                        );
                    }

                    break;
            }

            await _notificationRealtime.SaveChangesAsync();

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
        public async Task<IActionResult> CancelMySosRequest(string id)
        {
            var userId = _currentUser.UserId;

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

            await AddNewRequestNotificationsToAllStaffAsync(
                request,
                type: "StaffCaseCancelled",
                title: "คำขอที่รอรับงานถูกยกเลิก",
                message: $"เคส #{request.Id} ถูกผู้ใช้งานยกเลิกแล้ว ไม่ต้องรับหรือดำเนินการเคสนี้"
            );

            await _notificationRealtime.SaveChangesAsync();

            return Ok(new
            {
                message = "ยกเลิกคำขอความช่วยเหลือสำเร็จ"
            });
        }


        private async Task AddNewRequestNotificationsToAllStaffAsync(
            SosRequest request,
            string type,
            string title,
            string message)
        {
            var staffIds = await _context.Staffs
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

            if (staffIds.Count == 0)
            {
                return;
            }

            var nextNotificationId =
                await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.Notifications,
                    x => x.Id,
                    "Notification"
                );

            var normalizedMessage = message.Length > 500
                ? message[..497] + "..."
                : message;

            foreach (var staffId in staffIds)
            {
                _context.Notifications.Add(
                    new FloodRelief.Models.Notification
                    {
                        Id = nextNotificationId,
                        StaffId = staffId,
                        Type = type,
                        Title = title,
                        Message = normalizedMessage,
                        ReferenceType = "SosRequest",
                        ReferenceId = request.Id,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    }
                );

                nextNotificationId =
                    PrimaryKeyHelper.IncrementId(
                        nextNotificationId,
                        "Notification"
                    );
            }
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
        public async Task<IActionResult> GetSosStatistics(
            [FromQuery] string? centerId)
        {
            var query = _context.SosRequests
                .AsNoTracking()
                .AsQueryable();

            if (_currentUser.IsInRole("Staff"))
            {
                var staffCenterId = _currentUser.CenterId;

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
        public async Task<IActionResult> GetPendingSosRequests()
        {
            var query = _context.SosRequests
                .AsNoTracking()
                .Where(x => x.Status == SosRequestStatuses.Pending)
                .AsQueryable();

            if (_currentUser.IsInRole("Staff"))
            {
                var centerId = _currentUser.CenterId;

                if (string.IsNullOrWhiteSpace(centerId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบรหัสศูนย์จาก Token"
                    });
                }

                query = query.Where(x =>
                    x.CenterId == null ||
                    x.CenterId == centerId
                );
            }

            var requests = await query
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
                RequestType = x.RequestType,
                ReceiveMethod = x.ReceiveMethod,
                EmergencyType = x.EmergencyType,
                VictimCount = x.VictimCount,
                ChildCount = x.ChildCount,
                ElderlyCount = x.ElderlyCount,
                DisabledCount = x.DisabledCount,
                PatientCount = x.PatientCount,
                    DeathCount = x.DeathCount,
                Severity = x.Severity,
                WaterLevel = x.WaterLevel,
                EmergencyDetail = x.EmergencyDetail,
                Items = x.Items.Select(i => new SosRequestItemDto
                {
                    Id = i.Id,
                    ReliefItemId = i.ReliefItemId,
                    ReliefItemName = i.ReliefItem != null ? i.ReliefItem.Name : "ไม่ระบุ",
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList(),
                Priority = x.Priority,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

            var sortedRequests = requests
                .OrderBy(x => GetPriorityOrder(x.Priority))
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(sortedRequests);
        }
        // GET: api/sos-requests/staff/me
        public async Task<IActionResult> GetMyAssignedSosRequests()
        {
            var staffId = _currentUser.UserId;

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
                .Where(x =>
                    x.AssignedStaffId == staffId
                )
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

                    AssignedStaffId =
                        x.AssignedStaffId,

                    AssignedStaffName =
                        x.AssignedStaff != null
                            ? x.AssignedStaff.FullName
                            : null,

                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AddressDetail = x.AddressDetail,
                    RequestType = x.RequestType,
                    ReceiveMethod = x.ReceiveMethod,
                    EmergencyType = x.EmergencyType,
                    VictimCount = x.VictimCount,
                    ChildCount = x.ChildCount,
                    ElderlyCount = x.ElderlyCount,
                    DisabledCount = x.DisabledCount,
                    PatientCount = x.PatientCount,
                    DeathCount = x.DeathCount,
                    Severity = x.Severity,
                    WaterLevel = x.WaterLevel,
                    EmergencyDetail = x.EmergencyDetail,
                    Items = x.Items.Select(i => new SosRequestItemDto
                    {
                        Id = i.Id,
                        ReliefItemId = i.ReliefItemId,
                        ReliefItemName = i.ReliefItem != null ? i.ReliefItem.Name : "ไม่ระบุ",
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    }).ToList(),
                    Priority = x.Priority,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            var sortedRequests = requests
                .OrderBy(x =>
                    GetStatusOrder(x.Status)
                )
                .ThenBy(x =>
                    GetPriorityOrder(x.Priority)
                )
                .ThenByDescending(x =>
                    x.CreatedAt
                )
                .ToList();

            return Ok(sortedRequests);
        }
        // GET: api/sos-requests/center/001
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
            if (_currentUser.IsInRole("Staff"))
            {
                var staffCenterId = _currentUser.CenterId;

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
                    RequestType = x.RequestType,
                    ReceiveMethod = x.ReceiveMethod,
                    EmergencyType = x.EmergencyType,
                    VictimCount = x.VictimCount,
                    ChildCount = x.ChildCount,
                    ElderlyCount = x.ElderlyCount,
                    DisabledCount = x.DisabledCount,
                    PatientCount = x.PatientCount,
                    DeathCount = x.DeathCount,
                    Severity = x.Severity,
                    WaterLevel = x.WaterLevel,
                    EmergencyDetail = x.EmergencyDetail,
                    Items = x.Items.Select(i => new SosRequestItemDto
                    {
                        Id = i.Id,
                        ReliefItemId = i.ReliefItemId,
                        ReliefItemName = i.ReliefItem != null ? i.ReliefItem.Name : "ไม่ระบุ",
                        Quantity = i.Quantity,
                        Unit = i.Unit
                    }).ToList(),
                    Priority = x.Priority,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }
        public async Task<IActionResult> CheckStockBeforeAccept(
    string id, string? requestedCenterId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    message = "ไม่พบรหัสคำขอความช่วยเหลือ"
                });
            }

            // ==========================================
            // 1. หา Center ของ Staff ที่ Login อยู่
            // ==========================================

            var centerId = _currentUser.CenterId;

            if (string.IsNullOrWhiteSpace(centerId))
            {
                return BadRequest(new
                {
                    message = "ไม่พบศูนย์ของเจ้าหน้าที่"
                });
            }

            // ==========================================
            // 2. หา SOS พร้อมรายการสิ่งของที่ร้องขอ
            // ==========================================

            var sosRequest = await _context.SosRequests
                .AsNoTracking()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ReliefItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sosRequest == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบคำขอความช่วยเหลือ"
                });
            }

            // ==========================================
            // 3. ต้องยังเป็นเคสที่รับได้
            // ==========================================

            if (!string.Equals(
                sosRequest.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    message = "คำขอนี้ถูกรับงานหรือดำเนินการแล้ว"
                });
            }

            if (string.Equals(
                sosRequest.RequestType,
                "Emergency",
                StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new SosStockCheckResponseDto
                {
                    SosRequestId = sosRequest.Id,
                    CenterId = centerId,
                    IsAllEnough = true,
                    Items = new List<SosStockCheckItemDto>()
                });
            }

            // ==========================================
            // 4. เอา ReliefItemId ที่ SOS ต้องการ
            // ==========================================

            var reliefItemIds = sosRequest.Items
                .Select(x => x.ReliefItemId)
                .Distinct()
                .ToList();

            // ==========================================
            // 5. ดึง Inventory ของศูนย์ Staff
            // ==========================================

            var inventories = await _context.CenterInventories
                .AsNoTracking()
                .Where(x =>
                    x.CenterId == centerId &&
                    reliefItemIds.Contains(x.ReliefItemId))
                .ToListAsync();

            // ==========================================
            // 6. เปรียบเทียบ SOS กับ Inventory
            // ==========================================

            var items = sosRequest.Items
                .Select(item =>
                {
                    var inventory = inventories
                        .FirstOrDefault(x =>
                            x.ReliefItemId ==
                            item.ReliefItemId);

                    var requestedQuantity =
                        item.Quantity;

                    var availableQuantity =
                        inventory?.Quantity ?? 0;

                    var isEnough =
                        availableQuantity >=
                        requestedQuantity;

                    return new SosStockCheckItemDto
                    {
                        ReliefItemId =
                            item.ReliefItemId,

                        ReliefItemName =
                            item.ReliefItem?.Name
                            ?? "ไม่ระบุ",

                        Unit =
                            item.Unit ?? "",

                        RequestedQuantity =
                            requestedQuantity,

                        AvailableQuantity =
                            availableQuantity,

                        RemainingQuantity =
                            Math.Max(
                                availableQuantity -
                                requestedQuantity,
                                0),

                        ShortageQuantity =
                            Math.Max(
                                requestedQuantity -
                                availableQuantity,
                                0),

                        IsEnough =
                            isEnough
                    };
                })
                .ToList();

            // ==========================================
            // 7. ต้องพอทุกรายการ
            // ==========================================

            var isAllEnough =
                items.Count > 0 &&
                items.All(x => x.IsEnough);

            var response =
                new SosStockCheckResponseDto
                {
                    SosRequestId =
                        sosRequest.Id,

                    CenterId =
                        centerId,

                    IsAllEnough =
                        isAllEnough,

                    Items =
                        items
                };

            return Ok(response);
        }
        private static int GetPriorityOrder(string? priority)
        {
            var normalizedPriority =
                priority?.Trim().ToLowerInvariant();

            return normalizedPriority switch
            {
                "critical" => 1,
                "urgent" => 2,
                "normal" => 3,
                _ => 4
            };
        }
        private static int GetStatusOrder(
        string? status
)
        {
            var normalizedStatus =
                status?.Trim().ToLowerInvariant();

            return normalizedStatus switch
            {
                "delivering" => 1,
                "preparing" => 2,
                "accepted" => 3,
                "completed" => 4,
                "cancelled" => 5,
                _ => 99
            };
        }
    }
}
