using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
[Route("api/[controller]")]
[ApiController]
public class ReviewController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    // Khách hàng gửi đánh giá
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostReview([FromBody] ReviewUpsertDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // LOGIC CHỐT HẠ: Kiểm tra khách đã mua sản phẩm này chưa
        // Lưu ý: Phần này sẽ hoạt động khi bạn có bảng Orders ở Phase sau
        /*
        var hasBought = await _context.OrderItems
            .AnyAsync(oi => oi.Order.UserId == userId && oi.ProductId == dto.ProductId);
        if (!hasBought) return BadRequest("Bạn phải mua sản phẩm mới được đánh giá.");
        */

        var review = new ProductReview
        {
            ProductId = dto.ProductId,
            UserId = userId ?? "Anonymous",
            UserName = User.Identity?.Name ?? "Guest",
            Rating = dto.Rating,
            Comment = dto.Comment,
            ReviewImages = dto.Images ?? new(),
            IsApproved = false // Chờ Admin duyệt để tránh spam
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();
        return Ok("Đánh giá của bạn đã được gửi và đang chờ kiểm duyệt.");
    }

    // Lấy danh sách đánh giá của 1 sản phẩm (Public)
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Tính toán nhanh điểm trung bình (Rollup)
        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        return Ok(new
        {
            averageRating = Math.Round(averageRating, 1),
            totalReviews = reviews.Count,
            reviews
        });
    }

    // Admin duyệt đánh giá
    [Authorize(Roles = "Admin")]
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var review = await _context.ProductReviews.FindAsync(id);
        if (review == null) return NotFound();

        review.IsApproved = true;
        await _context.SaveChangesAsync();
        return Ok("Đã duyệt đánh giá.");
    }
}