using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string ReceiverName { get; set; } = string.Empty; // 👈 MỚI

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ReceiverPhone { get; set; } = string.Empty; // 👈 MỚI

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string DeliveryAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD"; // COD hoặc VNPAY
        public string? Note { get; set; }
    }
}