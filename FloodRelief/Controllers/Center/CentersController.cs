using FloodRelief.Data;
using FloodRelief.DTOs.Center;
using FloodRelief.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services.Center;

namespace FloodRelief.Controllers.Center
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentersController : ControllerBase
    {
        private readonly CentersService _service;

        public CentersController(CentersService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCenters()
        {
            return await _service.GetCenters();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCenterById(
        string id)
        {
            return await _service.GetCenterById(id);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCenter(
        [FromBody] CreateCenterRequestDto dto)
        {
            return await _service.CreateCenter(dto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCenter(
        string id,
        [FromBody] UpdateCenterRequestDto dto)
        {
            return await _service.UpdateCenter(id, dto);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCenter(
        string id)
        {
            return await _service.DeleteCenter(id);
        }
    }
}
