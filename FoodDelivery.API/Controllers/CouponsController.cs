using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System.Security.Claims;
=======
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
<<<<<<< HEAD
    [Authorize] // Mặc định yêu cầu đăng nhập
    public class CouponsController(AppDbContext context) : ControllerBase
    {
        // ============================================================
        // 🟢 PHẦN 1: API PUBLIC (Check mã)
        // ============================================================
=======
    // 👇 Vẫn giữ bảo mật Admin cho toàn bộ Class
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CouponsController(AppDbContext context) : ControllerBase
    {
        // ============================================================
        // 🟢 PHẦN 1: API CHO KHÁCH HÀNG (PUBLIC)
        // ============================================================

        // 👇 Thêm [AllowAnonymous] để Khách (chưa đăng nhập) cũng check được mã
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckCoupon([FromQuery] string code, [FromQuery] decimal orderValue)
        {
<<<<<<< HEAD
            if (string.IsNullOrEmpty(code)) return BadRequest("Vui lòng nhập mã.");
            var normalizedCode = code.Trim().ToUpper();

            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode && c.IsActive);
            if (coupon == null) return BadRequest("Mã giảm giá không tồn tại hoặc đã bị khóa.");

            var now = DateTime.UtcNow;
            if (now < coupon.StartDate || now > coupon.EndDate) return BadRequest("Mã giảm giá chưa bắt đầu hoặc đã hết hạn.");
            if (coupon.UsedCount >= coupon.UsageLimit) return BadRequest("Mã giảm giá đã hết lượt sử dụng.");
            if (orderValue < coupon.MinOrderValue) return BadRequest($"Đơn hàng phải từ {coupon.MinOrderValue:N0}đ trở lên.");

            decimal discountAmount = 0;
            if (string.Equals(coupon.DiscountType, "FIXED", StringComparison.OrdinalIgnoreCase))
                discountAmount = coupon.DiscountValue;
            else if (string.Equals(coupon.DiscountType, "PERCENT", StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = orderValue * (coupon.DiscountValue / 100);
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                    discountAmount = coupon.MaxDiscountAmount.Value;
            }

            if (discountAmount > orderValue) discountAmount = orderValue;
=======
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
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

            return Ok(new
            {
                coupon.Code,
<<<<<<< HEAD
                coupon.Id,
=======
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
                DiscountAmount = discountAmount,
                FinalTotal = orderValue - discountAmount,
                Message = "Áp dụng mã thành công!"
            });
        }

        // ============================================================
<<<<<<< HEAD
        // 🔵 PHẦN 2: API MEMBER (Lưu mã)
        // ============================================================
        public class SaveCouponRequest { public string Code { get; set; } = string.Empty; }

        [HttpPost("save")]
        public async Task<IActionResult> SaveCoupon([FromBody] SaveCouponRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var normalizedCode = request.Code.Trim().ToUpper();
            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode);

            // Logic validation ngắn gọn
            if (coupon == null || !coupon.IsActive) return BadRequest("Mã không hợp lệ.");

            var existing = await context.UserCoupons.AnyAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id);
            if (existing) return BadRequest("Bạn đã lưu mã này rồi.");

            context.UserCoupons.Add(new UserCoupon { UserId = userId, CouponId = coupon.Id, IsUsed = false, DateSaved = DateTime.UtcNow });
            await context.SaveChangesAsync();
            return Ok(new { message = "Lưu mã thành công!" });
        }

        [HttpGet("my-coupons")]
        public async Task<IActionResult> GetMyCoupons()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var myCoupons = await context.UserCoupons
                .Include(uc => uc.Coupon)
                .Where(uc => uc.UserId == userId)
                .ToListAsync(); // Lấy về trước rồi sort ở client để tránh lỗi EF Core query phức tạp

            var result = myCoupons.Select(uc => new {
                uc.Coupon!.Id,
                uc.Coupon!.Code,
                uc.Coupon!.Title,
                uc.Coupon!.DiscountValue,
                uc.Coupon!.DiscountType,
                Status = uc.IsUsed ? "used" : (uc.Coupon!.EndDate < DateTime.UtcNow ? "expired" : "valid")
            }).OrderByDescending(x => x.Status == "valid");

            return Ok(result);
        }

        // ============================================================
        // 🔴 PHẦN 3: API ADMIN (Chỉ Admin - Đã bỏ SuperAdmin)
        // ============================================================

        // 1. Lấy danh sách
        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> GetCoupons()
        //{
        //    var coupons = await context.Coupons.OrderByDescending(c => c.Id).ToListAsync();
        //    return Ok(coupons);
        //}
        // [Authorize(Roles = "Admin")]  <-- Tạm thời comment dòng này lại
        [Authorize] // Chỉ cần đăng nhập là được vào
        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
            // 1. Tự kiểm tra quyền bằng code (Debug dễ hơn)
            var role = User.FindFirst("role")?.Value; // Tìm role tên ngắn

            // Nếu không thấy role ngắn, thử tìm role dài chuẩn Microsoft
            if (string.IsNullOrEmpty(role))
            {
                role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            // 2. Nếu Role không phải Admin -> Trả về lỗi kèm lý do (Để bạn biết đường sửa)
            if (role != "Admin" && role != "SuperAdmin")
            {
                return StatusCode(403, new
                {
                    Message = "Bị chặn do thiếu quyền Admin",
                    YourRole = role ?? "Không tìm thấy role nào trong Token",
                    User = User.Identity?.Name
                });
            }

            // 3. Nếu qua được cửa ải trên -> Lấy dữ liệu
=======
        // 🔴 PHẦN 2: API CHO ADMIN (CRUD) - Giữ nguyên logic cũ
        // ============================================================

        // 1. GET ALL
        [HttpGet]
        public async Task<IActionResult> GetCoupons()
        {
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            var coupons = await context.Coupons.OrderByDescending(c => c.Id).ToListAsync();
            return Ok(coupons);
        }

<<<<<<< HEAD
        // 2. Lấy chi tiết
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCoupon(int id)
        {
            var coupon = await context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            return Ok(coupon);
        }

        // 3. Tạo mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCoupon([FromBody] Coupon coupon)
        {
            coupon.Code = coupon.Code.Trim().ToUpper();
            if (await context.Coupons.AnyAsync(c => c.Code == coupon.Code)) return BadRequest("Mã đã tồn tại.");

            coupon.UsedCount = 0;
            coupon.IsActive = true;
            context.Coupons.Add(coupon);
            await context.SaveChangesAsync();
            return Ok(coupon);
        }

        // 4. Cập nhật
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] Coupon request)
        {
            if (id != request.Id) return BadRequest();
            var coupon = await context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            // Map dữ liệu (Giữ nguyên ID và UsedCount)
            coupon.Code = request.Code.ToUpper();
            coupon.Title = request.Title;
            coupon.Description = request.Description;
            coupon.DiscountType = request.DiscountType;
            coupon.DiscountValue = request.DiscountValue;
            coupon.MinOrderValue = request.MinOrderValue;
            coupon.MaxDiscountAmount = request.MaxDiscountAmount;
            coupon.StartDate = request.StartDate;
            coupon.EndDate = request.EndDate;
            coupon.UsageLimit = request.UsageLimit;
            coupon.IsActive = request.IsActive;

            await context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 5. Xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
=======
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
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

<<<<<<< HEAD
            // Soft delete nếu đã có người dùng
            if (coupon.UsedCount > 0)
            {
                coupon.IsActive = false; // Ẩn đi
                await context.SaveChangesAsync();
                return Ok(new { message = "Mã đã được sử dụng -> Đã ẩn (không xóa vĩnh viễn)." });
=======
            if (coupon.UsedCount > 0)
            {
                coupon.IsActive = false;
                await context.SaveChangesAsync();
                return Ok(new { message = "Mã đã có người dùng -> Chuyển sang ẩn (IsActive=false)." });
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            }

            context.Coupons.Remove(coupon);
            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa vĩnh viễn." });
        }
    }
<<<<<<< HEAD
}
=======
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
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
