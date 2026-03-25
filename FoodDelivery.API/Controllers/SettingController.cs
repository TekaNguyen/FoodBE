using FoodDelivery.API.DTOs;
using FoodDelivery.API.Services;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class SettingController(AppDbContext context, IEmailService emailService) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;

    // --- 1. EMAIL CONFIGURATION (SMTP) ---

    // Đổi tên endpoint cho khớp với cURL bạn đã dùng: email-config
    [HttpGet("email-config")]
    public async Task<ActionResult<EmailSettingDto>> GetEmailSettings()
    {
        var settings = await _context.EmailSettings.FirstOrDefaultAsync();
        if (settings == null) return NotFound("Chưa có cấu hình Email.");

        var dto = new EmailSettingDto
        {
            Host = settings.Host,
            Port = settings.Port,
            Email = settings.Email,
            DisplayName = settings.DisplayName,
            EnableSsl = settings.EnableSsl
        };

        return Ok(dto);
    }

    // Đổi tên endpoint cho khớp với cURL bạn đã dùng: email-config
    [HttpPost("email-config")]
    public async Task<IActionResult> UpdateEmailSettings([FromBody] EmailSetting settings)
    {
        var existing = await _context.EmailSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.EmailSettings.Add(settings);
        }
        else
        {
            existing.Host = settings.Host;
            existing.Port = settings.Port;
            existing.Email = settings.Email;
            existing.Password = settings.Password;
            existing.DisplayName = settings.DisplayName;
            existing.EnableSsl = settings.EnableSsl;
        }

        await _context.SaveChangesAsync();
        return Ok("Đã cập nhật cấu hình SMTP thành công.");
    }

    // --- 2. EMAIL TEMPLATES ---

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        // Đảm bảo tên DbSet trong AppDbContext là EmailTemplates (có chữ s)
        var templates = await _context.EmailTemplates.ToListAsync();
        return Ok(templates);
    }

    [HttpPut("templates/{key}")]
    public async Task<IActionResult> UpdateTemplate(string key, [FromBody] EmailTemplateUpdateDto dto)
    {
        // Sử dụng Key đồng bộ với Domain
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Key == key);
        if (template == null) return NotFound("Template không tồn tại.");

        template.Subject = dto.Subject ?? string.Empty;
        template.Body = dto.Body ?? string.Empty;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật Template thành công", data = template });
    }

    // --- 3. TEST SEND EMAIL ---

    [HttpPost("test-send-email")]
    [AllowAnonymous] // Để bạn dễ dàng test từ Swagger không cần login Admin
    public async Task<IActionResult> TestEmail([FromQuery] string targetEmail)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "UserName", "Khách Hàng Test" },
            { "ResetLink", "https://fooddelivery.com/reset-password?token=123" }
        };

        try
        {
            await _emailService.SendTemplateEmailAsync(targetEmail, "FORGOT_PASSWORD", placeholders);
            return Ok("Email đã được gửi đi thành công! Kiểm tra hộp thư của bạn.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Gửi mail thất bại: {ex.Message}");
        }
    }
}