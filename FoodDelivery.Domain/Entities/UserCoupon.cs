using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    // Bảng trung gian: Lưu trữ việc "User A đang sở hữu Coupon B"
    public class UserCoupon
    {
        [Key]
        public int Id { get; set; }

        // 1. Liên kết với User (Khách hàng)
        [Required]
        public string? UserId { get; set; } // ID của IdentityUser thường là String (Guid)

        // 2. Liên kết với Coupon (Mã giảm giá)
        [Required]
        public int CouponId { get; set; }

        // 3. Trạng thái sử dụng
        public bool IsUsed { get; set; } = false; // False: Chưa dùng (còn trong ví), True: Đã dùng

        // Ngày lưu mã vào ví
        public DateTime DateSaved { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties (Để Entity Framework hiểu quan hệ) ---

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; } // Giả định bạn đã có class User kế thừa IdentityUser

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }
    }
}