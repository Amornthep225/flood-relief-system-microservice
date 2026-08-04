using FloodRelief.Services.Common;
using FloodRelief.Helpers;
using FloodRelief.Data;
using FloodRelief.DTOs.Donation;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;
namespace FloodRelief.Services.Donations
{
    public class DonationsService : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUser;

        public DonationsService(
            AppDbContext context,
            CurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }


        //[Authorize(Roles = "User")]
        public async Task<IActionResult> CreateDonation(
    CreateDonationDto dto)
        {
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้งาน"
                });
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest(new
                {
                    message = "กรุณาระบุรายการสิ่งของบริจาค"
                });
            }

            var donationId =
                await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.Donations,
                    x => x.Id,
                    "Donation");

            var donation = new Donation
            {
                Id = donationId,
                UserId = userId,
                CenterId = dto.CenterId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            var nextDonationItemId =
                await PrimaryKeyHelper.GenerateNextIdAsync(
                    _context.DonationItems,
                    x => x.Id,
                    "DonationItem");

            foreach (var item in dto.Items)
            {
                var reliefItem =
                    await _context.ReliefItems
                        .FirstOrDefaultAsync(x =>
                            x.Id == item.ReliefItemId
                        );

                if (reliefItem == null)
                {
                    return BadRequest(new
                    {
                        message =
                            $"ไม่พบรายการสิ่งของรหัส {item.ReliefItemId}"
                    });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "จำนวนสิ่งของต้องมากกว่า 0"
                    });
                }

                donation.Items.Add(
                    new DonationItem
                    {
                        Id = nextDonationItemId,
                        ReliefItemId = item.ReliefItemId,
                        Quantity = item.Quantity,
                        Unit = reliefItem.Unit
                    }
                );

                nextDonationItemId =
                    PrimaryKeyHelper.IncrementId(
                        nextDonationItemId,
                        "DonationItem");
            }

            // URL ที่จะเปิดเมื่อสแกน QR Code
            const string FrontendUrl = "http://localhost:3000";

            if (string.IsNullOrWhiteSpace(FrontendUrl))
            {
                return StatusCode(500, new
                {
                    message = "FrontendUrl ยังไม่ได้ตั้งค่า"
                });
            }

            var trackingUrl =
            $"{FrontendUrl}/user/donor-tracking?id={Uri.EscapeDataString(donation.Id)}";

            using var qrGenerator =
                new QRCodeGenerator();

            using var qrData =
                qrGenerator.CreateQrCode(
                    trackingUrl,
                    QRCodeGenerator.ECCLevel.Q
                );

            var qrCodeBase64 =
                new Base64QRCode(qrData)
                    .GetGraphic(20);

            donation.QRCode =
                $"data:image/png;base64,{qrCodeBase64}";

            _context.Donations.Add(donation);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ส่งข้อมูลบริจาคสำเร็จ",
                donationId = donation.Id,
                trackingUrl,
                qrCode = donation.QRCode
            });
        }



        public async Task<IActionResult> GetMyDonations()
        {
            var userId = _currentUser.UserId;


            var donations = await _context.Donations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.DonationType,
                    x.Description,
                    x.Quantity,
                    x.Unit,
                    x.ImageUrl,
                    x.Status,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();


            return Ok(donations);
        }



        public async Task<IActionResult> GetAllDonations()
        {
            var donations = await _context.Donations
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,

                    UserId = x.UserId,

                    DonorName = x.User!.FullName,

                    PhoneNumber = x.User.PhoneNumber,

                    x.DonationType,

                    x.Description,

                    x.Quantity,

                    x.Unit,

                    x.ImageUrl,

                    x.Status,

                    x.CreatedAt,

                    x.UpdatedAt
                })
                .ToListAsync();


            return Ok(donations);
        }



        //[Authorize]
        public async Task<IActionResult> GetDonationById(string id)
        {
            var donation = await _context.Donations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DonationDetailDto
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

                    UserEmail =
                        x.User != null
                        ? x.User.Email
                        : string.Empty,



                    CenterId = x.CenterId,

                    CenterName =
                        x.Center != null
                        ? x.Center.CenterName
                        : null,

                    CenterPhoneNumber =
                        x.Center != null
                        ? x.Center.PhoneNumber
                        : null,



                    Status = x.Status,

                    ImageUrl = x.ImageUrl,
                    QRCode = x.QRCode,

                    CreatedAt = x.CreatedAt,

                    UpdatedAt = x.UpdatedAt,



                    Items = x.Items
                        .Select(item => new DonationItemDto
                        {
                            Id = item.Id,

                            ReliefItemId =
                                item.ReliefItemId,


                            ReliefItemName =
                                item.ReliefItem != null
                                ? item.ReliefItem.Name
                                : string.Empty,


                            Quantity = item.Quantity,


                            Unit = item.Unit
                        })
                        .ToList()

                })
                .FirstOrDefaultAsync();



            if (donation == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบข้อมูลบริจาค"
                });
            }



            return Ok(donation);
        }



        public async Task<IActionResult> UpdateStatus(
    string id,
    UpdateDonationStatusDto dto)
        {
            var status =
                dto.Status?.Trim();

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new
                {
                    message = "กรุณาระบุสถานะ"
                });
            }

            if (string.Equals(
                status,
                "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "กรุณาใช้ API รับของเข้าคลัง POST /api/donations/{id}/receive"
                });
            }

            var allowedStatuses =
                new[]
                {
            "Pending",
            "Cancelled",
            "Rejected"
                };

            var normalizedStatus =
                allowedStatuses.FirstOrDefault(x =>
                    string.Equals(
                        x,
                        status,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (normalizedStatus == null)
            {
                return BadRequest(new
                {
                    message = "สถานะไม่ถูกต้อง",
                    allowedStatuses
                });
            }

            var donation =
                await _context.Donations
                    .FirstOrDefaultAsync(x =>
                        x.Id == id
                    );

            if (donation == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบข้อมูลบริจาค"
                });
            }

            if (string.Equals(
                donation.Status,
                "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "รายการที่รับเข้าคลังแล้วไม่สามารถเปลี่ยนสถานะด้วย API นี้ได้"
                });
            }

            donation.Status =
                normalizedStatus;

            donation.UpdatedAt =
                DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "อัปเดตสถานะสำเร็จ",
                donationId = donation.Id,
                status = donation.Status
            });
        }

        // ค้นหารายการบริจาคสำหรับหน้าสแกน/รับของเข้าคลัง
        // รองรับทั้ง Donation ID โดยตรง, DONATION:0000000001
        // และ URL ติดตามที่มี query string ?id=0000000001
        public async Task<IActionResult> SearchForReceive(string value)
        {
            var donationId = ExtractDonationId(value);

            if (string.IsNullOrWhiteSpace(donationId))
            {
                return BadRequest(new
                {
                    message = "กรุณาระบุ Donation ID หรือสแกน QR Code"
                });
            }

            if (!PrimaryKeyHelper.IsValidId(donationId))
            {
                return BadRequest(new
                {
                    message = "Donation ID ต้องเป็นตัวเลข 10 หลัก"
                });
            }

            var donation = await _context.Donations
                .AsNoTracking()
                .Where(x => x.Id == donationId)
                .Select(x => new DonationReceiveLookupDto
                {
                    Id = x.Id,
                    Status = x.Status,
                    UserId = x.UserId,
                    DonorName = x.User != null
                        ? x.User.FullName
                        : string.Empty,
                    DonorPhoneNumber = x.User != null
                        ? x.User.PhoneNumber
                        : string.Empty,
                    CenterId = x.CenterId,
                    CenterName = x.Center != null
                        ? x.Center.CenterName
                        : string.Empty,
                    CreatedAt = x.CreatedAt,
                    CanReceive = x.Status == "Pending",
                    Items = x.Items
                        .Select(item => new DonationReceiveLookupItemDto
                        {
                            DonationItemId = item.Id,
                            ReliefItemId = item.ReliefItemId,
                            ReliefItemName = item.ReliefItem != null
                                ? item.ReliefItem.Name
                                : string.Empty,
                            Quantity = item.Quantity,
                            Unit = item.Unit
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (donation == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบข้อมูลบริจาค",
                    donationId
                });
            }

            if (_currentUser.IsInRole("Staff"))
            {
                var staffCenterId = _currentUser.CenterId;

                if (string.IsNullOrWhiteSpace(staffCenterId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบข้อมูลศูนย์ของเจ้าหน้าที่ใน Token"
                    });
                }

                if (!string.Equals(
                    staffCenterId,
                    donation.CenterId,
                    StringComparison.Ordinal))
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message = "เจ้าหน้าที่สามารถค้นหาและรับของได้เฉพาะศูนย์ของตนเอง"
                        }
                    );
                }
            }

            if (string.Equals(
                donation.Status,
                "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    message = "รายการบริจาคนี้ถูกรับเข้าคลังแล้ว",
                    donation
                });
            }

            return Ok(donation);
        }

        private static string? ExtractDonationId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var input = value.Trim();

            const string prefix = "DONATION:";
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return input[prefix.Length..].Trim();
            }

            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                var query = uri.Query.TrimStart('?');

                foreach (var part in query.Split(
                    '&',
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    var pair = part.Split('=', 2);

                    if (pair.Length == 2 &&
                        string.Equals(
                            pair[0],
                            "id",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(pair[1]).Trim();
                    }
                }
            }

            return input;
        }

        // รับของบริจาคเข้าคลัง
        // POST /api/donations/{id}/receive
        public async Task<IActionResult> ReceiveDonation(string id)
        {
            id = id.Trim();

            if (!PrimaryKeyHelper.IsValidId(id))
            {
                return BadRequest(new
                {
                    message = "Donation ID ต้องเป็นตัวเลข 10 หลัก"
                });
            }

            var staffId = _currentUser.UserId;

            var donation = await _context.Donations
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (donation == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบข้อมูลบริจาค"
                });
            }

            // ป้องกันการรับของซ้ำ
            if (string.Equals(
                donation.Status,
                "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "รายการบริจาคนี้ถูกรับเข้าคลังแล้ว"
                });
            }

            if (!string.Equals(
                donation.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        $"ไม่สามารถรับของได้ เนื่องจากสถานะปัจจุบันคือ {donation.Status}"
                });
            }

            if (donation.Items == null ||
                donation.Items.Count == 0)
            {
                return BadRequest(new
                {
                    message = "รายการบริจาคไม่มีสิ่งของ"
                });
            }

            // Staff รับของได้เฉพาะศูนย์ตัวเอง
            // Admin สามารถรับแทนได้
            if (_currentUser.IsInRole("Staff"))
            {
                if (string.IsNullOrWhiteSpace(staffId))
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบข้อมูลเจ้าหน้าที่"
                    });
                }

                var staff = await _context.Staffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == staffId &&
                        x.IsActive
                    );

                if (staff == null)
                {
                    return Unauthorized(new
                    {
                        message = "ไม่พบข้อมูลเจ้าหน้าที่"
                    });
                }

                if (staff.CenterId != donation.CenterId)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new
                        {
                            message =
                                "เจ้าหน้าที่สามารถรับของได้เฉพาะศูนย์ของตนเอง"
                        }
                    );
                }
            }

            await using var databaseTransaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var nextInventoryId =
                    await PrimaryKeyHelper.GenerateNextIdAsync(
                        _context.CenterInventories,
                        x => x.Id,
                        "CenterInventory");

                var nextTransactionId =
                    await PrimaryKeyHelper.GenerateNextIdAsync(
                        _context.InventoryTransactions,
                        x => x.Id,
                        "InventoryTransaction");

                var receivedItems = new List<object>();

                foreach (var donationItem in donation.Items)
                {
                    if (donationItem.Quantity <= 0)
                    {
                        await databaseTransaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message =
                                $"จำนวนสิ่งของรหัส {donationItem.ReliefItemId} ไม่ถูกต้อง"
                        });
                    }

                    var reliefItemExists =
                        await _context.ReliefItems
                            .AnyAsync(x =>
                                x.Id == donationItem.ReliefItemId
                            );

                    if (!reliefItemExists)
                    {
                        await databaseTransaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message =
                                $"ไม่พบรายการสิ่งของรหัส {donationItem.ReliefItemId}"
                        });
                    }

                    var inventory =
                        await _context.CenterInventories
                            .FirstOrDefaultAsync(x =>
                                x.CenterId == donation.CenterId &&
                                x.ReliefItemId ==
                                    donationItem.ReliefItemId
                            );

                    if (inventory == null)
                    {
                        inventory = new CenterInventory
                        {
                            Id = nextInventoryId,
                            CenterId = donation.CenterId,
                            ReliefItemId =
                                donationItem.ReliefItemId,
                            Quantity = 0,
                            MinimumQuantity = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.CenterInventories.Add(
                            inventory
                        );

                        nextInventoryId =
                            PrimaryKeyHelper.IncrementId(
                                nextInventoryId,
                                "CenterInventory"
                            );
                    }

                    inventory.Quantity +=
                        donationItem.Quantity;

                    inventory.UpdatedAt =
                        DateTime.Now;

                    var inventoryTransaction =
                        new InventoryTransaction
                        {
                            Id = nextTransactionId,

                            CenterInventoryId =
                                inventory.Id,

                            TransactionType =
                                "DonationIn",

                            Quantity =
                                donationItem.Quantity,

                            BalanceAfter =
                                inventory.Quantity,

                            ReferenceType =
                                "Donation",

                            ReferenceId =
                                donation.Id,

                            Note =
                                $"รับของบริจาคเลขที่ {donation.Id}",

                            StaffId =
                                _currentUser.IsInRole("Staff")
                                    ? staffId
                                    : null,

                            CreatedAt =
                                DateTime.Now
                        };

                    _context.InventoryTransactions.Add(
                        inventoryTransaction
                    );

                    nextTransactionId =
                        PrimaryKeyHelper.IncrementId(
                            nextTransactionId,
                            "InventoryTransaction"
                        );

                    receivedItems.Add(new
                    {
                        donationItem.ReliefItemId,
                        quantityReceived =
                            donationItem.Quantity,
                        currentQuantity =
                            inventory.Quantity
                    });
                }

                donation.Status = "Received";
                donation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await databaseTransaction.CommitAsync();

                return Ok(new
                {
                    message =
                        "รับของบริจาคเข้าคลังสำเร็จ",

                    donationId =
                        donation.Id,

                    centerId =
                        donation.CenterId,

                    status =
                        donation.Status,

                    receivedAt =
                        donation.UpdatedAt,

                    items =
                        receivedItems
                });
            }
            catch (DbUpdateException)
            {
                await databaseTransaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "ไม่สามารถบันทึกข้อมูลเข้าคลังได้"
                });
            }
            catch (Exception)
            {
                await databaseTransaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "เกิดข้อผิดพลาดขณะรับของบริจาค"
                });
            }
        }

        public async Task<IActionResult> DeleteDonation(
            string id)
        {
            var donation =
                await _context.Donations
                .FirstOrDefaultAsync(x => x.Id == id);


            if (donation == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบข้อมูลบริจาค"
                });
            }


            _context.Donations.Remove(donation);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "ลบข้อมูลสำเร็จ"
            });
        }
    }
}
