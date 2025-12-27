using System;
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
        public string Code { get; set; } = string.Empty; // Mã giảm giá (VD: SALE50)

        // Loại giảm giá: "PERCENT" (Theo %) hoặc "FIXED" (Số tiền cụ thể)
        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "FIXED";

        // Giá trị giảm (Ví dụ: 10 = 10% hoặc 10 = 10.000đ tùy theo Type)
        [Column(TypeName = "decimal(18,2)")] // Định dạng tiền tệ chuẩn cho DB
        public decimal DiscountValue { get; set; }

        // Giá trị đơn hàng tối thiểu để được dùng mã (VD: Đơn > 100k mới được giảm)
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0;

        // Số tiền giảm tối đa (Chỉ dùng cho loại PERCENT. VD: Giảm 50% nhưng tối đa 20k)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        // Thời hạn sử dụng (Mặc định: Nay -> 30 ngày sau)
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);

        // Quản lý số lượng (Usage Limit)
        public int UsageLimit { get; set; } = 100; // Mặc định 100 lần
        public int UsedCount { get; set; } = 0;    // Đã dùng bao nhiêu lần

        public bool IsActive { get; set; } = true;
    }
}
