using FoodDelivery.API.Services;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = "Admin")] // Bật lại khi chạy thật
public class EmailSettingController(AppDbContext context, IEmailService emailService) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;

    // 1. Lấy cấu hình hiện tại
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _context.EmailSettings.FirstOrDefaultAsync();
        return Ok(settings ?? new EmailSetting());
    }

    // 2. Cập nhật cấu hình SMTP
    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] EmailSetting setting)
    {
        var existing = await _context.EmailSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.EmailSettings.Add(setting);
        }
        else
        {
            existing.Host = setting.Host;
            existing.Port = setting.Port;
            existing.Email = setting.Email;
            existing.Password = setting.Password;
            existing.DisplayName = setting.DisplayName;
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = "Email settings updated successfully!" });
    }

    // 3. Test gửi mail ngay lập tức
    [HttpPost("test")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail)
    {
        try
        {
            await _emailService.SendEmailAsync(toEmail, "Test Connection", "<h1>Mail Service is Working! 🚀</h1>");
            return Ok("Email sent successfully!");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed: {ex.Message}");
        }
    }
}