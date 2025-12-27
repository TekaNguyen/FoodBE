using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 👇 Vẫn giữ bảo mật Admin cho toàn bộ Class
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CouponsController(AppDbContext context) : ControllerBase
    {
        // ============================================================
        // 🟢 PHẦN 1: API CHO KHÁCH HÀNG (PUBLIC)
        // ============================================================

        // 👇 Thêm [AllowAnonymous] để Khách (chưa đăng nhập) cũng check được mã
        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckCoupon([FromQuery] string code, [FromQuery] decimal orderValue)
        {
            // 1. Tìm mã (Code phải chính xác, đang Active)
            var coupon = await context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

            if (coupon == null)
                return BadRequest("Mã giảm giá không tồn tại hoặc đã bị khóa.");

            // 2. Check thời hạn
            var now = DateTime.UtcNow;
            if (now < coupon.StartDate || now > coupon.EndDate)
                return BadRequest("Mã giảm giá chưa bắt đầu hoặc đã hết hạn.");

            // 3. Check lượt dùng
            if (coupon.UsedCount >= coupon.UsageLimit)
                return BadRequest("Mã giảm giá đã hết lượt sử dụng.");

            // 4. Check giá trị đơn tối thiểu
            if (orderValue < coupon.MinOrderValue)
                return BadRequest($"Đơn hàng phải từ {coupon.MinOrderValue:N0}đ trở lên mới được dùng mã này.");

            // 5. Tính toán số tiền giảm
            decimal discountAmount = 0;

            if (coupon.DiscountType == "FIXED")
            {
                discountAmount = coupon.DiscountValue;
            }
            else if (coupon.DiscountType == "PERCENT")
            {
                discountAmount = orderValue * (coupon.DiscountValue / 100);

                // Nếu có trần giảm tối đa (Max Cap)
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }

            // Không được giảm quá giá trị đơn hàng (tránh bị âm tiền)
            if (discountAmount > orderValue)
            {
                discountAmount = orderValue;
            }

            return Ok(new
            {
                coupon.Code,
                DiscountAmount = discountAmount,
                FinalTotal = orderValue - discountAmount,
                Message = "Áp dụng mã thành công!"
            });
        }

        // ============================================================
        // 🔴 PHẦN 2: API CHO ADMIN (CRUD) - Giữ nguyên logic cũ
        // ============================================================

        // 1. GET ALL
        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
            var coupons = await context.Coupons.OrderByDescending(c => c.Id).ToListAsync();
            return Ok(coupons);
        }

        // 2. GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCoupon(int id)
        {
            var coupon = await context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound("Không tìm thấy mã.");
            return Ok(coupon);
        }

        // 3. CREATE
        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] Coupon coupon)
        {
            coupon.Code = coupon.Code.ToUpper().Trim();

            if (await context.Coupons.AnyAsync(c => c.Code == coupon.Code))
                return BadRequest($"Mã '{coupon.Code}' đã tồn tại!");

            if (coupon.EndDate < coupon.StartDate)
                return BadRequest("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            if (coupon.DiscountType != "FIXED" && coupon.DiscountType != "PERCENT")
                return BadRequest("DiscountType phải là 'FIXED' hoặc 'PERCENT'.");

            coupon.UsedCount = 0;
            coupon.IsActive = true;

            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, coupon);
        }

        // 4. UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] Coupon request)
        {
            if (id != request.Id) return BadRequest("ID không khớp.");
            var existingCoupon = await context.Coupons.FindAsync(id);
            if (existingCoupon == null) return NotFound("Không tìm thấy mã.");

            existingCoupon.Code = request.Code.ToUpper().Trim();
            existingCoupon.DiscountType = request.DiscountType;
            existingCoupon.DiscountValue = request.DiscountValue;
            existingCoupon.MinOrderValue = request.MinOrderValue;
            existingCoupon.MaxDiscountAmount = request.MaxDiscountAmount;
            existingCoupon.StartDate = request.StartDate;
            existingCoupon.EndDate = request.EndDate;
            existingCoupon.UsageLimit = request.UsageLimit;
            existingCoupon.IsActive = request.IsActive;

            await context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!", data = existingCoupon });
        }

        // 5. DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            if (coupon.UsedCount > 0)
            {
                coupon.IsActive = false;
                await context.SaveChangesAsync();
                return Ok(new { message = "Mã đã có người dùng -> Chuyển sang ẩn (IsActive=false)." });
            }

            context.Coupons.Remove(coupon);
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa vĩnh viễn." });
        }
    }
}

//using FoodDelivery.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace FoodDelivery.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    // 👇 Primary Constructor: Khai báo 'context' ngay tại đây
//    public class CouponsController(AppDbContext context) : ControllerBase
//    {
//        // API: Kiểm tra mã giảm giá
//        [HttpGet("check")]
//        public async Task<IActionResult> CheckCoupon([FromQuery] string code, [FromQuery] decimal orderValue)
//        {
//            // 👇 Dùng trực tiếp 'context' (không cần _context)
//            var coupon = await context.Coupons
//                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

//            if (coupon == null)
//            {
//                return BadRequest("Mã giảm giá không tồn tại hoặc đã bị khóa.");
//            }

//            var now = DateTime.UtcNow;
//            if (now < coupon.StartDate || now > coupon.EndDate)
//            {
//                return BadRequest("Mã giảm giá chưa bắt đầu hoặc đã hết hạn.");
//            }

//            if (coupon.UsedCount >= coupon.UsageLimit)
//            {
//                return BadRequest("Mã giảm giá đã hết lượt sử dụng.");
//            }

//            if (orderValue < coupon.MinOrderValue)
//            {
//                return BadRequest($"Đơn hàng phải từ {coupon.MinOrderValue:N0}đ trở lên mới được dùng mã này.");
//            }

//            decimal discountAmount = 0;

//            if (coupon.DiscountType == "FIXED")
//            {
//                discountAmount = coupon.DiscountValue;
//            }
//            else if (coupon.DiscountType == "PERCENT")
//            {
//                discountAmount = orderValue * (coupon.DiscountValue / 100);

//                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
//                {
//                    discountAmount = coupon.MaxDiscountAmount.Value;
//                }
//            }

//            if (discountAmount > orderValue)
//            {
//                discountAmount = orderValue;
//            }

//            return Ok(new
//            {
//                coupon.Code, // Rút gọn Code = coupon.Code
//                DiscountAmount = discountAmount,
//                FinalTotal = orderValue - discountAmount,
//                Message = "Áp dụng mã thành công!"
//            });
//        }
//    }
//}