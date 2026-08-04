using System.Data;
using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/relief-items")]
    [ApiController]
    public class ReliefItemsController : ControllerBase
    {
        private readonly ReliefItemsService _service;

        public ReliefItemsController(ReliefItemsService service)
        {
            _service = service;
        }

        // GET: api/relief-items
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            return await _service.GetItems();
        }
        // GET: api/relief-items/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveItems()
        {
            return await _service.GetActiveItems();
        }
        // GET: api/relief-items/category/food
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetItemsByCategory(string categoryId)
        {
            return await _service.GetItemsByCategory(categoryId);
        }
        // GET: api/relief-items/00001
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(string id)
        {
            return await _service.GetItemById(id);
        }
        // POST: api/relief-items
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateItem(
        [FromBody]  CreateReliefItemDto dto)
        {
            return await _service.CreateItem(dto);
        }
        // PUT: api/relief-items/00001
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(
        string id,
        [FromBody] UpdateReliefItemDto dto)
        {
            return await _service.UpdateItem(id, dto);
        }
        // PUT: api/relief-items/00001/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItemStatus(
        string id,
        [FromBody] UpdateReliefItemStatusDto dto)
        {
            return await _service.UpdateItemStatus(id, dto);
        }
        // DELETE: api/relief-items/00001
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            return await _service.DeleteItem(id);
        }
    }
}
