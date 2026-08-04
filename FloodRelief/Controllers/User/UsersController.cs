using FloodRelief.Data;
using FloodRelief.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloodRelief.Services;

namespace FloodRelief.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _service;

        public UsersController(UsersService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return await _service.GetUsers();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            return await _service.GetUserById(id);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto dto)
        {
            return await _service.UpdateUser(id, dto);
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDto dto)
        {
            return await _service.UpdateUserStatus(id, dto);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            return await _service.DeleteUser(id);
        }
    }
}
