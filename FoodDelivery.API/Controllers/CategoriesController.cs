using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(AppDbContext context) : ControllerBase
{
    // 1. Lấy danh sách danh mục (Ai cũng xem được)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await context.Categories.ToListAsync();
    }

    // 2. Tạo danh mục mới (Chỉ Admin)
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        // Check trùng tên
        if (await context.Categories.AnyAsync(c => c.Name == category.Name))
        {
            return BadRequest("Danh mục này đã tồn tại!");
        }

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
        if (category == null) return NotFound();

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa danh mục!" });
    }

    [HttpPut("reorder")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<int> sortedIds)
    {
        // sortedIds: [5, 2, 1, 4] -> Nghĩa là ID 5 đứng đầu, ID 2 đứng nhì...

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