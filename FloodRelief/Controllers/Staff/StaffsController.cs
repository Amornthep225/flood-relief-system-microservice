using FloodRelief.Data;
using FloodRelief.DTOs.Staff;
using FloodRelief.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly StaffsService _service;

        public StaffsController(StaffsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffs()
        {
            return await _service.GetStaffs();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(string id)
        {
            return await _service.GetStaffById(id);
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetStaffSummary()
        {
            return await _service.GetStaffSummary();
        }
        [HttpPost]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequestDto dto)
        {
            return await _service.CreateStaff(dto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(string id, [FromBody] UpdateStaffRequestDto dto)
        {
            return await _service.UpdateStaff(id, dto);
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStaffStatus(string id, [FromBody] UpdateStaffStatusDto dto)
        {
            return await _service.UpdateStaffStatus(id, dto);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            return await _service.DeleteStaff(id);
        }
    }
}
