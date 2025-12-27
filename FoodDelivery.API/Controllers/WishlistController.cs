using System.Security.Claims;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Bắt buộc đăng nhập mới được thả tim
public class WishlistController(AppDbContext context) : ControllerBase
{
    // 1. Xem danh sách yêu thích
    [HttpGet]
    public async Task<IActionResult> GetMyWishlist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var wishlist = await context.Wishlists
            .Where(w => w.UserId == userId)
            .Include(w => w.Product) // Lấy thông tin món ăn
            .Select(w => new
            {
                w.ProductId,
                ProductName = w.Product!.Name,
                w.Product.Price,
                w.Product.ImageUrl,
                w.AddedAt
            })
            .ToListAsync();

        return Ok(wishlist);
    }

    // 2. Thả tim / Bỏ tim (Toggle)
    [HttpPost("toggle/{productId}")]
    public async Task<IActionResult> ToggleWishlist(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Kiểm tra món ăn có tồn tại không
        var productExists = await context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) return NotFound("Món ăn không tồn tại!");

        // Kiểm tra xem đã thích chưa
        var wishlistItem = await context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (wishlistItem != null)
        {
            // Đã thích rồi -> Xóa đi (Bỏ thích)
            context.Wishlists.Remove(wishlistItem);
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã bỏ yêu thích! 💔", isLiked = false });
        }
        else
        {
            // Chưa thích -> Thêm vào
            var newItem = new Wishlist
            {
                UserId = userId!,
                ProductId = productId
            };
            context.Wishlists.Add(newItem);
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm vào yêu thích! ❤️", isLiked = true });
        }
    }
}