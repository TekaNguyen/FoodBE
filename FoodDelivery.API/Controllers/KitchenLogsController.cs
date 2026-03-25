using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// 👇 Import Entity
using FoodDelivery.Domain.Entities;
// 👇 Import DbContext (Bạn kiểm tra namespace của AppDbContext, thường là cái này)
using FoodDelivery.Infrastructure.Persistence;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin,Chef")] // Bỏ comment khi đã phân quyền xong
    public class KitchenLogsController(AppDbContext context) : ControllerBase
    {
        // 1. Lấy danh sách nhật ký hôm nay
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayLogs()
        {
            var today = DateTime.Today;

            var logs = await context.KitchenProductionLogs
                .Where(x => x.CreatedAt >= today)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // 2. Tạo nhật ký mới
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] KitchenProductionLog model)
        {
            if (string.IsNullOrEmpty(model.ProductName))
            {
                return BadRequest("Tên món không được để trống");
            }

            model.CreatedAt = DateTime.Now;

            context.KitchenProductionLogs.Add(model);
            await context.SaveChangesAsync();

            return Ok(model);
        }
    }
}