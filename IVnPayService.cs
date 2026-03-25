namespace FoodDelivery.Application.Interfaces
{
    public interface IVnPayService
    {
        // Hàm tạo URL thanh toán VNPAY (Trả về URL/QR)
        string CreatePaymentUrl(int orderId, decimal amount, HttpContext context);

        // Hàm xử lý kết quả VNPAY gọi về (IPN - Trả về kết quả giao dịch)
        VnPayResponse ProcessPaymentReturn(IQueryCollection collections);
    }
}