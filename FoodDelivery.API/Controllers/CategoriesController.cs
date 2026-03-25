using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence; // Hoặc .Data tùy namespace của bạn

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(AppDbContext context) : ControllerBase
{
    // 1. Lấy danh sách danh mục (Ai cũng xem được)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        // 👇 SỬA 1: Thêm OrderBy để danh sách hiện đúng thứ tự đã sắp xếp
        return await context.Categories
                            .OrderBy(c => c.SortOrder)
                            .ToListAsync();
    }

    // 2. Tạo danh mục mới (Chỉ Admin)
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        // Check trùng tên
        if (await context.Categories.AnyAsync(c => c.Name == category.Name))
        {
            return BadRequest(new { message = "Tên danh mục này đã tồn tại!" }); // Trả về JSON message cho đẹp
        }

        // Gán thứ tự mặc định là cuối cùng
        var maxSortOrder = await context.Categories.MaxAsync(c => (int?)c.SortOrder) ?? 0;
        category.SortOrder = maxSortOrder + 1;

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    // 3. Xóa danh mục (Chỉ Admin)
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null) return NotFound(new { message = "Không tìm thấy danh mục" });

        // 👇 SỬA 2: Kiểm tra xem danh mục có đang chứa món ăn nào không?
        // Giả sử bảng món ăn tên là "Products"
        bool hasProducts = await context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return BadRequest(new { message = "Không thể xóa danh mục đang chứa món ăn. Hãy xóa hoặc chuyển món ăn sang danh mục khác trước!" });
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa danh mục!" });
    }

    // 4. Sắp xếp lại thứ tự
    [HttpPut("reorder")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<int> sortedIds)
    {
        var categories = await context.Categories.ToListAsync();

        for (int i = 0; i < sortedIds.Count; i++)
        {
            var cat = categories.FirstOrDefault(c => c.Id == sortedIds[i]);
            if (cat != null)
            {
                cat.SortOrder = i; // Gán index làm thứ tự
            }
        }

        await context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thứ tự thành công!" });
    }
}