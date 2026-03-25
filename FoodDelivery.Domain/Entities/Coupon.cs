using System;
using System.Collections.Generic; // 👇 Import thêm để dùng List
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // VD: SALE50

        // 👇 BỔ SUNG 1: Cần có tên/mô tả để hiển thị lên App cho khách đọc
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty; // VD: "Giảm 50% cho bạn mới"

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty; // VD: "Áp dụng tối đa 50k..."

        // Loại giảm giá: "PERCENT" hoặc "FIXED"
        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "FIXED";

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);

        public int UsageLimit { get; set; } = 100;
        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // 👇 BỔ SUNG 2: Quan hệ ngược về bảng UserCoupon
        // Để sau này có thể truy vấn: Coupon này những ai đã lưu?
        public ICollection<UserCoupon>? UserCoupons { get; set; }
    }
}