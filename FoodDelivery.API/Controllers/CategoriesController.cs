using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Domain.Entities;
<<<<<<< HEAD
using FoodDelivery.Infrastructure.Persistence; // Hoặc .Data tùy namespace của bạn
=======
using FoodDelivery.Infrastructure.Persistence;
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(AppDbContext context) : ControllerBase
{
    // 1. Lấy danh sách danh mục (Ai cũng xem được)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
<<<<<<< HEAD
        // 👇 SỬA 1: Thêm OrderBy để danh sách hiện đúng thứ tự đã sắp xếp
        return await context.Categories
                            .OrderBy(c => c.SortOrder)
                            .ToListAsync();
=======
        return await context.Categories.ToListAsync();
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    }

    // 2. Tạo danh mục mới (Chỉ Admin)
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        // Check trùng tên
        if (await context.Categories.AnyAsync(c => c.Name == category.Name))
        {
<<<<<<< HEAD
            return BadRequest(new { message = "Tên danh mục này đã tồn tại!" }); // Trả về JSON message cho đẹp
        }

        // Gán thứ tự mặc định là cuối cùng
        var maxSortOrder = await context.Categories.MaxAsync(c => (int?)c.SortOrder) ?? 0;
        category.SortOrder = maxSortOrder + 1;

=======
            return BadRequest("Danh mục này đã tồn tại!");
        }

>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
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
<<<<<<< HEAD
        if (category == null) return NotFound(new { message = "Không tìm thấy danh mục" });

        // 👇 SỬA 2: Kiểm tra xem danh mục có đang chứa món ăn nào không?
        // Giả sử bảng món ăn tên là "Products"
        bool hasProducts = await context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return BadRequest(new { message = "Không thể xóa danh mục đang chứa món ăn. Hãy xóa hoặc chuyển món ăn sang danh mục khác trước!" });
        }
=======
        if (category == null) return NotFound();
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa danh mục!" });
    }

<<<<<<< HEAD
    // 4. Sắp xếp lại thứ tự
=======
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    [HttpPut("reorder")]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<int> sortedIds)
    {
<<<<<<< HEAD
=======
        // sortedIds: [5, 2, 1, 4] -> Nghĩa là ID 5 đứng đầu, ID 2 đứng nhì...

>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
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