using FloodRelief.Data;
using FloodRelief.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Controllers;

[Route("api/public-statistics")]
[ApiController]
[AllowAnonymous]
public class PublicStatisticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PublicStatisticsController(
        AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/public-statistics/home
    [HttpGet("home")]
    public async Task<ActionResult<HomeStatisticsResponseDto>>
        GetHomeStatistics()
    {
        /*
         * UserId ใน Donation ของคุณเป็น Required string
         * จึงไม่ต้องตรวจ != null
         *
         * กรองค่าว่างเผื่อมีข้อมูลเก่าในฐานข้อมูล
         */
        var totalDonors =
            await _context.Donations
                .AsNoTracking()
                .Where(donation =>
                    donation.UserId != "")
                .Select(donation =>
                    donation.UserId)
                .Distinct()
                .CountAsync();

        /*
         * จากสถานะระบบเดิมของคุณ
         * เคสสำเร็จอาจใช้ Completed หรือ Delivered
         */
        var completedSosRequests =
            await _context.SosRequests
                .AsNoTracking()
                .CountAsync(request =>
                    request.Status == "Completed" ||
                    request.Status == "Delivered");

        var response =
            new HomeStatisticsResponseDto
            {
                TotalDonors =
                    totalDonors,

                CompletedSosRequests =
                    completedSosRequests,
            };

        return Ok(response);
    }
}