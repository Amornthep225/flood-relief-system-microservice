using System.Security.Claims;
using FloodRelief.Data;
using FloodRelief.DTOs.Center;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services.Center;

namespace FloodRelief.Controllers.Center
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenterInventoriesController : ControllerBase
    {
        private readonly CenterInventoriesService _service;

        public CenterInventoriesController(CenterInventoriesService service)
        {
            _service = service;
        }

        // ดูของทั้งหมดในศูนย์
        // GET /api/inventories/center/00001
        [HttpGet("center/{centerId}")]
        public async Task<IActionResult> GetCenterInventory(
        string centerId)
        {
            return await _service.GetCenterInventory(centerId);
        }
        [HttpGet]
        public async Task<IActionResult>
        GetCenterInventoryItem(
        string centerId,
        string reliefItemId)
        {
            return await _service.GetCenterInventoryItem(centerId, reliefItemId);
        }
        // เพิ่มของเข้าศูนย์
        // POST /api/inventories/stock-in
        [HttpPost("stock-in")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> StockIn(
        [FromBody]
        UpdateInventoryQuantityDto dto)
        {
            return await _service.StockIn(dto);
        }
        // นำของออกจากศูนย์
        // POST /api/inventories/stock-out
        [HttpPost("stock-out")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> StockOut(
        [FromBody]
        UpdateInventoryQuantityDto dto)
        {
            return await _service.StockOut(dto);
        }
        // แก้ไขจำนวนขั้นต่ำ
        // PUT /api/inventories/0000000001/minimum
        [HttpPut("{id}/minimum")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
        UpdateMinimumQuantity(
        string id,
        [FromBody]
        UpdateMinimumQuantityDto dto)
        {
            return await _service.UpdateMinimumQuantity(id, dto);
        }
        // ดูประวัติการเพิ่มและลด
        // GET /api/inventories/0000000001/transactions
        [HttpGet("{id}/transactions")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult>
        GetTransactions(string id)
        {
            return await _service.GetTransactions(id);
        }
        // ดูรายการของขาด
        // GET /api/inventories/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult>
        GetLowStock()
        {
            return await _service.GetLowStock();
        }
        // PUT /api/CenterInventories/0000000001/thresholds
        [HttpPut("{id}/thresholds")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateThresholds(
            string id,
            [FromBody] UpdateInventoryThresholdsDto dto
        )
        {
            return await _service.UpdateThresholds(id, dto);
        }
    }
}
