using FloodRelief.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers
{
    [ApiController]
    [Route("api/thai-addresses")]
    [Authorize]
    public class ThaiAddressesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThaiAddressesController(
            AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.ThaiProvinces
                .AsNoTracking()
                .OrderBy(x => x.NameTh)
                .Select(x => new
                {
                    id = x.Id,
                    nameTh = x.NameTh,
                    nameEn = x.NameEn
                })
                .ToListAsync();

            return Ok(provinces);
        }

        [HttpGet("provinces/{provinceId:int}/districts")]
        public async Task<IActionResult> GetDistricts(
            int provinceId)
        {
            var provinceExists =
                await _context.ThaiProvinces
                    .AnyAsync(x => x.Id == provinceId);

            if (!provinceExists)
            {
                return NotFound(new
                {
                    message = "ไม่พบจังหวัดที่เลือก"
                });
            }

            var districts = await _context.ThaiDistricts
                .AsNoTracking()
                .Where(x => x.ProvinceId == provinceId)
                .OrderBy(x => x.NameTh)
                .Select(x => new
                {
                    id = x.Id,
                    provinceId = x.ProvinceId,
                    nameTh = x.NameTh,
                    nameEn = x.NameEn
                })
                .ToListAsync();

            return Ok(districts);
        }

        [HttpGet("districts/{districtId:int}/sub-districts")]
        public async Task<IActionResult> GetSubDistricts(
            int districtId)
        {
            var districtExists =
                await _context.ThaiDistricts
                    .AnyAsync(x => x.Id == districtId);

            if (!districtExists)
            {
                return NotFound(new
                {
                    message = "ไม่พบอำเภอที่เลือก"
                });
            }

            var subDistricts =
                await _context.ThaiSubDistricts
                    .AsNoTracking()
                    .Where(x => x.DistrictId == districtId)
                    .OrderBy(x => x.NameTh)
                    .Select(x => new
                    {
                        id = x.Id,
                        districtId = x.DistrictId,
                        nameTh = x.NameTh,
                        nameEn = x.NameEn,
                        zipCode = x.ZipCode,
                        latitude = x.Latitude,
                        longitude = x.Longitude
                    })
                    .ToListAsync();

            return Ok(subDistricts);
        }
    }
}