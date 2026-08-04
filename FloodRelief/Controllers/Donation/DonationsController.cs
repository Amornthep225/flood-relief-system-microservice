using FloodRelief.Data;
using FloodRelief.DTOs.Donation;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QRCoder;
using FloodRelief.Services.Donations;

namespace FloodRelief.Controllers.Donations
{
    [Route("api/donations")]
    [ApiController]
    public class DonationsController : ControllerBase
    {
        private readonly DonationsService _service;

        public DonationsController(DonationsService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "User")]
        public async Task<IActionResult> CreateDonation(
        [FromBody] CreateDonationDto dto)
        {
            return await _service.CreateDonation(dto);
        }
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyDonations()
        {
            return await _service.GetMyDonations();
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllDonations()
        {
            return await _service.GetAllDonations();
        }
        [HttpGet("{id}")]
        //[Authorize]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> GetDonationById(string id)
        {
            return await _service.GetDonationById(id);
        }
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateDonationStatusDto dto)
        {
            return await _service.UpdateStatus(id, dto);
        }
        // ค้นหารายการบริจาคก่อนรับเข้าคลัง
        // ใช้ได้ทั้งการพิมพ์ Donation ID และข้อความที่อ่านจาก QR Code
        // GET /api/donations/receive/search?value=0000000001
        [HttpGet("receive/search")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> SearchForReceive(
            [FromQuery] string value)
        {
            return await _service.SearchForReceive(value);
        }

        // รับของบริจาคเข้าคลัง
        // POST /api/donations/{id}/receive
        [HttpPost("{id}/receive")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReceiveDonation(string id)
        {
            return await _service.ReceiveDonation(id);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDonation(
        string id)
        {
            return await _service.DeleteDonation(id);
        }
    }
}
