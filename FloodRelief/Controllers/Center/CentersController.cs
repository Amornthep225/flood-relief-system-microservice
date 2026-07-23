using FloodRelief.Data;
using FloodRelief.DTOs.Center;
using FloodRelief.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers.Center
{
    [Route("api/centers")]
    [ApiController]
    public class CentersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CentersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCenters()
        {
            var centers = await _context.Centers
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CenterName,
                    x.Address,
                    x.ProvinceId,
                    x.DistrictId,
                    x.SubDistrictId,
                    x.Province,
                    x.District,
                    x.SubDistrict,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.ContactName,
                    x.Latitude,
                    x.Longitude,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(centers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCenterById(
            string id)
        {
            var center = await _context.Centers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CenterName,
                    x.Address,
                    x.ProvinceId,
                    x.DistrictId,
                    x.SubDistrictId,
                    x.Province,
                    x.District,
                    x.SubDistrict,
                    x.ZipCode,
                    x.PhoneNumber,
                    x.ContactName,
                    x.Latitude,
                    x.Longitude,
                    x.IsActive,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (center == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });
            }

            return Ok(center);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCenter(
            [FromBody] CreateCenterRequestDto dto)
        {
            var addressData =
                await GetValidatedAddressAsync(
                    dto.ProvinceId,
                    dto.DistrictId,
                    dto.SubDistrictId
                );

            if (addressData == null)
            {
                return BadRequest(new
                {
                    message =
                        "ข้อมูลจังหวัด อำเภอ หรือตำบลไม่สัมพันธ์กัน"
                });
            }

            var center = new Models.Center
            {
                Id = await GenerateCenterIdAsync(),
                CenterName = dto.CenterName.Trim(),
                Address = dto.Address.Trim(),
                ProvinceId = addressData.District.ProvinceId,
                DistrictId = addressData.DistrictId,
                SubDistrictId = addressData.Id,
                Province = addressData.District.Province.NameTh,
                District = addressData.District.NameTh,
                SubDistrict = addressData.NameTh,
                ZipCode = addressData.ZipCode.ToString("D5"),
                PhoneNumber = dto.PhoneNumber.Trim(),
                ContactName = dto.ContactName.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Centers.Add(center);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message =
                        "ไม่สามารถสร้างศูนย์ได้ รหัสศูนย์อาจซ้ำ กรุณาลองใหม่อีกครั้ง"
                });
            }

            return CreatedAtAction(
                nameof(GetCenterById),
                new
                {
                    id = center.Id
                },
                new
                {
                    message = "สร้างศูนย์สำเร็จ",
                    centerId = center.Id,
                    centerName = center.CenterName
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCenter(
            string id,
            [FromBody] UpdateCenterRequestDto dto)
        {
            var center = await _context.Centers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (center == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });
            }

            var addressData =
                await GetValidatedAddressAsync(
                    dto.ProvinceId,
                    dto.DistrictId,
                    dto.SubDistrictId
                );

            if (addressData == null)
            {
                return BadRequest(new
                {
                    message =
                        "ข้อมูลจังหวัด อำเภอ หรือตำบลไม่สัมพันธ์กัน"
                });
            }

            center.CenterName = dto.CenterName.Trim();
            center.Address = dto.Address.Trim();
            center.ProvinceId = addressData.District.ProvinceId;
            center.DistrictId = addressData.DistrictId;
            center.SubDistrictId = addressData.Id;
            center.Province = addressData.District.Province.NameTh;
            center.District = addressData.District.NameTh;
            center.SubDistrict = addressData.NameTh;
            center.ZipCode = addressData.ZipCode.ToString("D5");
            center.PhoneNumber = dto.PhoneNumber.Trim();
            center.ContactName = dto.ContactName.Trim();
            center.Latitude = dto.Latitude;
            center.Longitude = dto.Longitude;
            center.IsActive = dto.IsActive;
            center.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "แก้ไขศูนย์สำเร็จ"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCenter(
            string id)
        {
            var center = await _context.Centers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (center == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });
            }

            if (!center.IsActive)
            {
                return BadRequest(new
                {
                    message = "ศูนย์นี้ถูกปิดใช้งานแล้ว"
                });
            }

            center.IsActive = false;
            center.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "ปิดใช้งานศูนย์สำเร็จ"
            });
        }

        private async Task<ThaiSubDistrict?>
            GetValidatedAddressAsync(
                int provinceId,
                int districtId,
                int subDistrictId)
        {
            var subDistrict = await _context.ThaiSubDistricts
                .AsNoTracking()
                .Include(x => x.District)
                .ThenInclude(x => x.Province)
                .FirstOrDefaultAsync(
                    x => x.Id == subDistrictId
                );

            if (subDistrict == null)
            {
                return null;
            }

            if (subDistrict.DistrictId != districtId)
            {
                return null;
            }

            if (subDistrict.District.ProvinceId != provinceId)
            {
                return null;
            }

            return subDistrict;
        }

        private async Task<string> GenerateCenterIdAsync()
        {
            var lastCenterId = await _context.Centers
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var nextId = 1;

            if (
                !string.IsNullOrWhiteSpace(lastCenterId) &&
                int.TryParse(lastCenterId, out var currentId)
            )
            {
                nextId = currentId + 1;
            }

            return nextId
                .ToString()
                .PadLeft(5, '0');
        }
    }
}
