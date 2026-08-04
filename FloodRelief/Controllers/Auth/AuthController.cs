using FloodRelief.Data;
using FloodRelief.DTOs.Auth;
using FloodRelief.DTOs.Staff;
using FloodRelief.Models;
using FloodRelief.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }

        [HttpPost("user-register")]
        public async Task<IActionResult> UserRegister(
        [FromBody] UserRegisterRequestDto dto)
        {
            return await _service.UserRegister(dto);
        }
        [HttpPost("user-login")]
        public async Task<IActionResult> UserLogin(
        [FromBody] UserLoginRequestDto dto)
        {
            return await _service.UserLogin(dto);
        }
        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin(
        [FromBody] StaffLoginRequestDto dto)
        {
            return await _service.StaffLogin(dto);
        }
        [HttpPost("staff-register")]
        public async Task<IActionResult> StaffRegister(
        [FromBody] CreateStaffRequestDto dto)
        {
            return await _service.StaffRegister(dto);
        }
        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin(
        [FromBody] AdminLoginRequestDto dto)
        {
            return await _service.AdminLogin(dto);
        }
        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister(
        [FromBody] AdminRegisterRequestDto dto)
        {
            return await _service.AdminRegister(dto);
        }
    }
}
