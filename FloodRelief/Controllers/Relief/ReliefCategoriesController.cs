using FloodRelief.Data;
using FloodRelief.DTOs.Relief;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/relief-categories")]
    [ApiController]
    public class ReliefCategoriesController : ControllerBase
    {
        private readonly ReliefCategoriesService _service;

        public ReliefCategoriesController(ReliefCategoriesService service)
        {
            _service = service;
        }

        // GET: /api/relief-categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            return await _service.GetCategories();
        }
        // GET: /api/relief-categories/active
        // สำหรับหน้า User และ Donor
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            return await _service.GetActiveCategories();
        }
        // GET: /api/relief-categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            return await _service.GetCategoryById(id);
        }
        // POST: /api/relief-categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(
        [FromBody]  CreateReliefCategoryDto dto)
        {
            return await _service.CreateCategory(dto);
        }
        // PUT: /api/relief-categories/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(
        string id,
        [FromBody] UpdateReliefCategoryDto dto)
        {
            return await _service.UpdateCategory(id, dto);
        }
        // PUT: /api/relief-categories/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategoryStatus(
        string id,
        [FromBody] UpdateReliefItemStatusDto dto)
        {
            return await _service.UpdateCategoryStatus(id, dto);
        }
        // DELETE: /api/relief-categories/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            return await _service.DeleteCategory(id);
        }
    }
}
