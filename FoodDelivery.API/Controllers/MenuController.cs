using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController(AppDbContext context) : ControllerBase
    {
        // 0. API Lấy Menu (QUAN TRỌNG: Phải lấy ra theo thứ tự SortOrder)
        [HttpGet("structure")]
        public async Task<IActionResult> GetMenuStructure()
        {
            var categories = await context.Categories
                .OrderBy(c => c.SortOrder) // Sắp xếp danh mục
                .Include(c => c.Products)  // Kèm món ăn
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.SortOrder,
                    // Sắp xếp món ăn trong danh mục
                    Products = c.Products.OrderBy(p => p.SortOrder).Select(p => new {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.SortOrder,
                        p.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // 1. API cập nhật thứ tự Danh mục (Categories)
        [HttpPost("reorder-categories")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReorderCategories([FromBody] List<int> sortedIds)
        {
            // sortedIds: Danh sách ID theo thứ tự mới từ Frontend gửi về
            for (int i = 0; i < sortedIds.Count; i++)
            {
                var cat = await context.Categories.FindAsync(sortedIds[i]);
                // 👇 Đã sửa thành SortOrder
                if (cat != null) cat.SortOrder = i;
            }
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã lưu thứ tự danh mục!" });
        }

        // 2. API cập nhật thứ tự Món ăn (Products)
        [HttpPost("reorder-products")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReorderProducts([FromBody] List<int> sortedIds)
        {
            for (int i = 0; i < sortedIds.Count; i++)
            {
                var product = await context.Products.FindAsync(sortedIds[i]);
                // 👇 Đã sửa thành SortOrder (Đảm bảo bảng Product có cột này)
                if (product != null) product.SortOrder = i;
            }
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã lưu thứ tự món ăn!" });
        }
    }
}