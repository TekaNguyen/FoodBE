using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.API.DTOs;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingsController(AppDbContext context) : ControllerBase
{
    // ==========================================
    // 1. LẤY TẤT CẢ CẤU HÌNH (Public)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        // Trả về dạng Dictionary cho Frontend dễ dùng: settings['hotline']
        var settings = await context.Settings.ToListAsync();
        return Ok(settings);
    }

    // ==========================================
    // 2. LẤY THEO KEY (Public)
    // Ví dụ: GET /api/settings/shipping_fee
    // ==========================================
    [HttpGet("{key}")]
    public async Task<IActionResult> GetSettingByKey(string key)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return NotFound(new { message = $"Không tìm thấy cấu hình '{key}'" });

        return Ok(setting);
    }

    // ==========================================
    // 3. TẠO MỚI (Admin Only)
    // ==========================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSettingDto request)
    {
        // Kiểm tra trùng Key
        if (await context.Settings.AnyAsync(s => s.Key == request.Key))
        {
            return BadRequest(new { message = $"Cấu hình '{request.Key}' đã tồn tại!" });
        }

        var setting = new Setting
        {
            Key = request.Key,
            Value = request.Value,
            Description = request.Description
        };

        context.Settings.Add(setting);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSettingByKey), new { key = setting.Key }, setting);
    }

    // ==========================================
    // 4. CẬP NHẬT GIÁ TRỊ (Admin Only)
    // Ví dụ: Đổi phí ship -> PUT /api/settings/shipping_fee
    // ==========================================
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto request)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return NotFound(new { message = "Không tìm thấy cấu hình này!" });

        setting.Value = request.Value;
        if (!string.IsNullOrEmpty(request.Description))
        {
            setting.Description = request.Description;
        }

        await context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công!", data = setting });
    }

    // ==========================================
    // 5. XÓA (Admin Only - Cẩn thận khi dùng)
    // ==========================================
    [HttpDelete("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSetting(string key)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return NotFound();

        context.Settings.Remove(setting);
        await context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa cấu hình!" });
    }
}