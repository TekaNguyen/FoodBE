using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.API.Helpers; // Nhớ thêm dòng này

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController(AppDbContext context) : ControllerBase
    {
        // 2. Gán tham số vào biến _context
        private readonly AppDbContext _context = context;

    

        [HttpGet]
        public async Task<IActionResult> GetPages()
        {
            var pages = await _context.Pages
                .Select(p => new { p.Id, p.Title, p.Slug, p.IsPublished, p.CreatedAt })
                .ToListAsync();
            return Ok(pages);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetPageBySlug(string slug)
        {
            var page = await _context.Pages.FirstOrDefaultAsync(p => p.Slug == slug);
            if (page == null) return NotFound("Trang không tồn tại.");

            if (!page.IsPublished && !User.IsInRole("Admin"))
                return NotFound("Trang chưa được xuất bản.");

            return Ok(page);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] PageDto dto)
        {
            // Kiểm tra trùng Slug
            if (await _context.Pages.AnyAsync(p => p.Slug == dto.Slug))
                return BadRequest("Slug (URL) này đã tồn tại.");

            var page = new Page
            {
                Title = dto.Title,
                Slug = SlugHelper.Generate(dto.Slug), // Dùng Helper, hết lỗi Null
                ContentJson = dto.ContentJson,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                IsPublished = dto.IsPublished
            };

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPageBySlug), new { slug = page.Slug }, page);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePage(int id, [FromBody] PageDto dto)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();

            if (page.Slug != dto.Slug && await _context.Pages.AnyAsync(p => p.Slug == dto.Slug))
                return BadRequest("Slug mới bị trùng.");

            page.Title = dto.Title;
            page.Slug = SlugHelper.Generate(dto.Slug); // Dùng Helper
            page.ContentJson = dto.ContentJson;
            page.MetaTitle = dto.MetaTitle;
            page.MetaDescription = dto.MetaDescription;
            page.IsPublished = dto.IsPublished;
            page.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(page);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa trang.");
        }

        // Đã XÓA hàm GenerateSlug cũ đi rồi nhé
    }

    // DTO đã được gán giá trị mặc định để tránh null
    public class PageDto
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ContentJson { get; set; } = "[]";
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public bool IsPublished { get; set; }
    }
}