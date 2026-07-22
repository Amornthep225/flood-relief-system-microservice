using FloodRelief.Data;
using FloodRelief.DTOs.Center;
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
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CenterName,
                    x.Address,
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
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CenterName,
                    x.Address,
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
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });


            return Ok(center);
        }



        [HttpPost]
        public async Task<IActionResult> CreateCenter(
            [FromBody] CreateCenterRequestDto dto)
        {
            var lastCenter = await _context.Centers
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();


            int nextId = 1;


            if (lastCenter != null)
            {
                nextId = int.Parse(lastCenter.Id) + 1;
            }


            string newCenterId =
                nextId.ToString()
                .PadLeft(5, '0');



            var center = new FloodRelief.Models.Center
            {
                Id = newCenterId,
                CenterName = dto.CenterName,
                Address = dto.Address,
                Province = dto.Province,
                District = dto.District,
                SubDistrict = dto.SubDistrict,
                ZipCode = dto.ZipCode,
                PhoneNumber = dto.PhoneNumber,
                ContactName = dto.ContactName,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsActive = true,
                CreatedAt = DateTime.Now
            };


            _context.Centers.Add(center);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "สร้างศูนย์สำเร็จ",
                centerId = center.Id,
                centerName = center.CenterName
            });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCenter(
            string id,
            [FromBody] CreateCenterRequestDto dto)
        {
            var center =
                await _context.Centers.FindAsync(id);


            if (center == null)
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });



            center.CenterName = dto.CenterName;
            center.Address = dto.Address;
            center.Province = dto.Province;
            center.District = dto.District;
            center.SubDistrict = dto.SubDistrict;
            center.ZipCode = dto.ZipCode;
            center.PhoneNumber = dto.PhoneNumber;
            center.ContactName = dto.ContactName;
            center.Latitude = dto.Latitude;
            center.Longitude = dto.Longitude;
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
            var center =
                await _context.Centers.FindAsync(id);


            if (center == null)
                return NotFound(new
                {
                    message = "ไม่พบศูนย์"
                });



            center.IsActive = false;
            center.UpdatedAt = DateTime.Now;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "ปิดใช้งานศูนย์สำเร็จ"
            });
        }
    }
}