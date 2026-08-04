using System.Data;
using System.Security.Claims;
using FloodRelief.Constants;
using FloodRelief.Data;
using FloodRelief.DTOs.Sos;
using FloodRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/sos-requests")]
    [ApiController]
    [Authorize]
    public class SosRequestsController : ControllerBase
    {
        private readonly SosRequestsService _service;

        public SosRequestsController(SosRequestsService service)
        {
            _service = service;
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyRequests()
        {
            return await _service.GetMyRequests();
        }
        // POST: api/sos-requests
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateSosRequest(
        [FromBody]  CreateSosRequestDto dto)
        {
            return await _service.CreateSosRequest(dto);
        }
        // GET: api/sos-requests/my
        // GET: api/sos-requests/my
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMySosRequests(
        DateTime? startDate,
        DateTime? endDate,
        string? status)
        {
            return await _service.GetMySosRequests(startDate, endDate, status);
        }
        // GET: api/sos-requests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllSosRequests(
        [FromQuery] string? status,
        [FromQuery] string? centerId)
        {
            return await _service.GetAllSosRequests(status, centerId);
        }
        // GET: api/sos-requests/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSosRequestById(string id)
        {
            return await _service.GetSosRequestById(id);
        }
        // PUT: api/sos-requests/{id}/assign
        // PUT: api/sos-requests/{id}/assign
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignSosRequest(
        string id,
        [FromBody] AssignSosRequestDto dto)
        {
            return await _service.AssignSosRequest(id, dto);
        }
        // PUT: api/sos-requests/{id}/status
        // PUT: api/sos-requests/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateSosStatus(
        string id,
        [FromBody] UpdateSosStatusDto dto)
        {
            return await _service.UpdateSosStatus(id, dto);
        }
        // PUT: api/sos-requests/{id}/cancel
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelMySosRequest(string id)
        {
            return await _service.CancelMySosRequest(id);
        }
        // GET: api/sos-requests/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSosStatistics(
        [FromQuery] string? centerId)
        {
            return await _service.GetSosStatistics(centerId);
        }
        // GET: api/sos-requests/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPendingSosRequests()
        {
            return await _service.GetPendingSosRequests();
        }
        // GET: api/sos-requests/staff/me
        [HttpGet("staff/me")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetMyAssignedSosRequests()
        {
            return await _service.GetMyAssignedSosRequests();
        }
        // GET: api/sos-requests/center/001
        [HttpGet("center/{centerId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetSosRequestsByCenter(
        string centerId)
        {
            return await _service.GetSosRequestsByCenter(centerId);
        }
    }
}
