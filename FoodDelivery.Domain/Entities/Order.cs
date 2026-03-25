using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Shipping, Completed, Cancelled

        public string? Note { get; set; }

        // 👇 THÔNG TIN NGƯỜI NHẬN (Bắt buộc cho Guest Checkout)
        // Vì khách vãng lai không có Profile, ta phải lưu tên và SĐT vào đây
        public string FullName { get; set; } = string.Empty;     // Tên người nhận
        public string PhoneNumber { get; set; } = string.Empty;  // SĐT liên hệ
        public string DeliveryAddress { get; set; } = string.Empty; // Địa chỉ giao hàng

        // 👇 LIÊN KẾT USER (Cho phép Null)
        public string? UserId { get; set; } // Khách vãng lai thì cái này = null
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        // 👇 SHIPPER
        public string? ShipperId { get; set; }

        // 👇 1. Phương thức thanh toán (COD, VNPAY, MOMO...)
        public string PaymentMethod { get; set; } = "COD";

        // 👇 2. Trạng thái thanh toán (Unpaid, Paid)
        public string PaymentStatus { get; set; } = "Unpaid";

        // 👇 3. Phí ship (Lúc nãy mình nói lưu lại, giờ làm luôn cho chuẩn)
        public decimal ShippingFee { get; set; } = 0;

        // 👇 CHI TIẾT ĐƠN HÀNG (Cú pháp C# 12)
        public List<OrderDetail> OrderDetails { get; set; } = [];

        // 👇 THÊM 2 TRƯỜNG NÀY
        public string? CouponCode { get; set; } // Mã giảm giá đã dùng (cho phép null)
        public decimal DiscountAmount { get; set; } = 0; // Số tiền được giảm

        // 👇 THÊM DÒNG NÀY ĐỂ SỬA LỖI 2
        public DateTime? PaymentDate { get; set; }
    }
}
