using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.API.Helpers;      // Đã thêm Helper
using FoodDelivery.API.DTOs;    // Đã thêm DTO

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // --- CATEGORIES ---

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _context.BlogCategories.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
        {
            var cat = new BlogCategory
            {
                Name = dto.Name,
                Slug = SlugHelper.Generate(dto.Name) // Dùng Helper
            };
            _context.BlogCategories.Add(cat);
            await _context.SaveChangesAsync();
            return Ok(cat);
        }

        // --- POSTS ---

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts(int page = 1, int size = 10, int? categoryId = null)
        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .Where(p => p.IsPublished && (p.PublishedAt == null || p.PublishedAt <= DateTime.UtcNow));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var total = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Thumbnail,
                    p.Summary,
                    p.PublishedAt,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(new { Total = total, Data = posts });
        }

        [HttpGet("posts/{slug}")]
        public async Task<IActionResult> GetPostDetail(string slug)
        {
            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null) return NotFound();

            post.ViewCount++;
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var post = new BlogPost
            {
                Title = dto.Title,
                Slug = SlugHelper.Generate(dto.Title), // Dùng Helper
                Thumbnail = dto.Thumbnail,
                Summary = dto.Summary,
                Content = dto.Content,
                IsPublished = dto.IsPublished,
                PublishedAt = dto.PublishedAt,
                CategoryId = dto.CategoryId,
                SeoTitle = dto.SeoTitle,
                SeoDescription = dto.SeoDescription,
                SeoKeywords = dto.SeoKeywords
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("posts/{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] CreatePostDto dto)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = dto.Title;
            // post.Slug = SlugHelper.Generate(dto.Title); // Uncomment nếu muốn đổi slug
            post.Thumbnail = dto.Thumbnail;
            post.Summary = dto.Summary;
            post.Content = dto.Content;
            post.IsPublished = dto.IsPublished;
            post.PublishedAt = dto.PublishedAt;
            post.CategoryId = dto.CategoryId;
            post.SeoTitle = dto.SeoTitle;
            post.SeoDescription = dto.SeoDescription;
            post.SeoKeywords = dto.SeoKeywords;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("posts/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }

        // ĐÃ XÓA HÀM GenerateSlug Ở ĐÂY VÌ ĐÃ DÙNG SlugHelper
    }
}