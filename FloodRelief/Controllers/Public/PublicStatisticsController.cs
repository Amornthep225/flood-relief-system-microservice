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
        GetHomeStatistics(
            CancellationToken cancellationToken)
    {
        /*
         * นับผู้บริจาคไม่ซ้ำจาก UserId
         *
         * ถ้า Donation ของคุณใช้ชื่อ DonorId
         * ให้เปลี่ยน donation.UserId เป็น donation.DonorId
         */
        var totalDonors = await _context.Donations
            .Where(donation =>
                donation.UserId != null)
            .Select(donation =>
                donation.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        /*
         * รองรับชื่อสถานะทั้ง Completed และ Delivered
         */
        var completedSosRequests =
            await _context.SosRequests
                .CountAsync(
                    request =>
                        request.Status == "Completed" ||
                        request.Status == "Delivered",
                    cancellationToken);

        return Ok(
            new HomeStatisticsResponseDto
            {
                TotalDonors = totalDonors,
                CompletedSosRequests =
                    completedSosRequests,
            });
    }
}