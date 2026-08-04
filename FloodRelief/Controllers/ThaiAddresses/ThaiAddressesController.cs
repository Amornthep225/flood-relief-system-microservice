using FloodRelief.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThaiAddressesController : ControllerBase
    {
        private readonly ThaiAddressesService _service;

        public ThaiAddressesController(ThaiAddressesService service)
        {
            _service = service;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            return await _service.GetProvinces();
        }
        [HttpGet("provinces/{provinceId:int}/districts")]
        public async Task<IActionResult> GetDistricts(
        int provinceId)
        {
            return await _service.GetDistricts(provinceId);
        }
        [HttpGet("districts/{districtId:int}/sub-districts")]
        public async Task<IActionResult> GetSubDistricts(
        int districtId)
        {
            return await _service.GetSubDistricts(districtId);
        }
    }
}
