namespace FoodDelivery.API.DTOs // 👈 Khai báo Namespace chuẩn
{
    public class CheckoutRequest
    {
        // Các trường bắt buộc (Non-nullable) -> Phải gán = string.Empty
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty; // "COD" hoặc "PAYOS"

        // Các trường tùy chọn (Nullable) -> Thêm dấu ?
        public string? Note { get; set; }
        public string? CouponCode { get; set; }
    }
}