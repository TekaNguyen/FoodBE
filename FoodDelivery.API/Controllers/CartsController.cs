using FoodDelivery.API.DTOs;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController(AppDbContext context) : ControllerBase
    {
        // =========================================================
        // 🛠️ HÀM PHỤ: Xác định người dùng là User hay Guest
        // =========================================================
        private (string? userId, string? sessionId) GetUserIdentity()
        {
            // 1. Check Token (User đã đăng nhập)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                return (userId, null); // Là User
            }

            // 2. Check Header (Khách vãng lai)
            if (Request.Headers.TryGetValue("X-Guest-ID", out var guestId))
            {
                return (null, guestId.ToString()); // Là Guest
            }

            return (null, null); // Không xác định
        }

        // =========================================================
        // 1. THÊM VÀO GIỎ
        // =========================================================
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
        {
            var (userId, sessionId) = GetUserIdentity();

            if (userId == null && sessionId == null)
                return BadRequest("Vui lòng đăng nhập hoặc gửi kèm Header 'X-Guest-ID'");

            // Kiểm tra món ăn
            var product = await context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Món ăn không tồn tại!");

            // Lấy Options
            var selectedOptions = new List<ProductOption>();
            if (request.OptionIds != null && request.OptionIds.Count > 0)
            {
                selectedOptions = await context.ProductOptions
                    .Where(o => request.OptionIds.Contains(o.Id))
                    .ToListAsync();
            }

            // Tạo CartItem (Điền vào cột phù hợp)
            var cartItem = new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,

                // 👇 Logic quan trọng nằm ở đây:
                UserId = userId,       // Nếu là User thì lưu vào đây
                SessionId = sessionId, // Nếu là Guest thì lưu vào đây

                SelectedOptions = []
            };

            foreach (var option in selectedOptions)
            {
                cartItem.SelectedOptions.Add(new CartItemOption { ProductOptionId = option.Id });
            }

            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            return Ok(new { message = "Đã thêm vào giỏ hàng!", cartItemId = cartItem.Id });
        }

        // =========================================================
        // 2. XEM GIỎ HÀNG
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var (userId, sessionId) = GetUserIdentity();
            if (userId == null && sessionId == null) return Unauthorized();

            // Query Database: Lấy theo UserId HOẶC SessionId
            var query = context.CartItems
                .Include(c => c.Product)
                .Include(c => c.SelectedOptions)
                    .ThenInclude(co => co.ProductOption)
                .AsQueryable();

            if (userId != null)
                query = query.Where(c => c.UserId == userId);
            else
                query = query.Where(c => c.SessionId == sessionId);

            var cartItems = await query.ToListAsync();

            // Map ra DTO
            var itemsDto = cartItems.Select(c =>
            {
                decimal basePrice = c.Product?.Price ?? 0;
                decimal optionPrice = c.SelectedOptions.Sum(o => o.ProductOption?.PriceModifier ?? 0);

                return new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product?.Name ?? "Unknown",
                    ProductImage = c.Product?.ImageUrl ?? "",
                    Quantity = c.Quantity,
                    UnitPrice = basePrice + optionPrice,
                    SelectedOptions = [.. c.SelectedOptions.Select(o => o.ProductOption?.Name ?? "")]
                };
            }).ToList();

            return Ok(new
            {
                Items = itemsDto,
                TotalAmount = itemsDto.Sum(i => i.TotalPrice),
                ItemCount = itemsDto.Count
            });
        }

        // =========================================================
        // 3. XÓA MÓN
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var (userId, sessionId) = GetUserIdentity();
            if (userId == null && sessionId == null) return Unauthorized();

            var item = await context.CartItems.FindAsync(id);
            if (item == null) return NotFound("Không tìm thấy món!");

            // Bảo mật: Kiểm tra xem món này có đúng của người đang gọi không
            bool isOwner = (userId != null && item.UserId == userId) ||
                           (sessionId != null && item.SessionId == sessionId);

            if (!isOwner) return Forbid();

            context.CartItems.Remove(item);
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa thành công!" });
        }
    }
}

//using FoodDelivery.API.DTOs;
//using FoodDelivery.Domain.Entities;
//using FoodDelivery.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Mvc; // Bỏ Authorize ở đây
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace FoodDelivery.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    // ⚠️ QUAN TRỌNG: Bỏ [Authorize] ở đây để Guest vào được
//    public class CartsController(AppDbContext context) : ControllerBase
//    {
//        // ==========================================
//        // 🛠️ HÀM PHỤ: Lấy ID từ Token HOẶC từ Header
//        // ==========================================
//        private string? GetCurrentUserId()
//        {
//            // 1. Ưu tiên: Lấy từ Token (nếu đã đăng nhập)
//            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (!string.IsNullOrEmpty(userIdFromToken))
//            {
//                return userIdFromToken;
//            }

//            // 2. Nếu không có Token: Lấy từ Header "X-Guest-ID"
//            // (Frontend phải tự sinh mã này và gửi lên)
//            if (Request.Headers.TryGetValue("X-Guest-ID", out var guestId))
//            {
//                return guestId.ToString();
//            }

//            // 3. Không có cả 2 -> Trả về null (Lỗi)
//            return null;
//        }

//        // ==========================================
//        // 1. THÊM VÀO GIỎ
//        // ==========================================
//        [HttpPost("add")]
//        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//                return BadRequest("Vui lòng đăng nhập hoặc gửi kèm X-Guest-ID!");

//            var product = await context.Products.FindAsync(request.ProductId);
//            if (product == null) return NotFound("Món ăn không tồn tại!");

//            var selectedOptions = new List<ProductOption>();
//            if (request.OptionIds != null && request.OptionIds.Count > 0)
//            {
//                selectedOptions = await context.ProductOptions
//                    .Where(o => request.OptionIds.Contains(o.Id))
//                    .ToListAsync();
//            }

//            // Tìm xem món này (với đúng option đó) đã có trong giỏ chưa?
//            // (Đoạn này nâng cao: Tạm thời cứ thêm mới, bài sau tối ưu gộp dòng)
//            var cartItem = new CartItem
//            {
//                ProductId = request.ProductId,
//                UserId = userId, // Lưu guest-id hoặc user-id đều được
//                Quantity = request.Quantity,
//                SelectedOptions = new List<CartItemOption>()
//            };

//            foreach (var option in selectedOptions)
//            {
//                cartItem.SelectedOptions.Add(new CartItemOption
//                {
//                    ProductOptionId = option.Id
//                });
//            }

//            context.CartItems.Add(cartItem);
//            await context.SaveChangesAsync();

//            return Ok(new { message = "Đã thêm vào giỏ hàng!", cartItemId = cartItem.Id });
//        }

//        // ==========================================
//        // 2. XEM GIỎ HÀNG
//        // ==========================================
//        [HttpGet]
//        public async Task<IActionResult> GetMyCart()
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId))
//                return BadRequest("Thiếu thông tin định danh!");

//            var cartItems = await context.CartItems
//                .Include(c => c.Product)
//                .Include(c => c.SelectedOptions)
//                    .ThenInclude(co => co.ProductOption)
//                .Where(c => c.UserId == userId)
//                .AsNoTracking()
//                .ToListAsync();

//            var itemsDto = cartItems.Select(c =>
//            {
//                decimal basePrice = c.Product?.Price ?? 0;
//                decimal optionPrice = c.SelectedOptions.Sum(o => o.ProductOption?.PriceModifier ?? 0);

//                return new CartItemDto
//                {
//                    Id = c.Id,
//                    ProductId = c.ProductId,
//                    ProductName = c.Product?.Name ?? "Unknown",
//                    ProductImage = c.Product?.ImageUrl ?? "",
//                    Quantity = c.Quantity,
//                    UnitPrice = basePrice + optionPrice,
//                    SelectedOptions = [.. c.SelectedOptions.Select(o => o.ProductOption?.Name ?? "")]
//                };
//            }).ToList();

//            return Ok(new
//            {
//                Items = itemsDto,
//                TotalAmount = itemsDto.Sum(i => i.TotalPrice),
//                ItemCount = itemsDto.Count
//            });
//        }

//        // ... (Hàm Delete giữ nguyên, chỉ sửa userId = GetCurrentUserId()) ...
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> RemoveFromCart(int id)
//        {
//            var userId = GetCurrentUserId();
//            if (string.IsNullOrEmpty(userId)) return Unauthorized();

//            var item = await context.CartItems
//                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

//            if (item == null) return NotFound("Không tìm thấy món!");

//            context.CartItems.Remove(item);
//            await context.SaveChangesAsync();
//            return Ok(new { message = "Đã xóa thành công!" });
//        }
//    }
//}
