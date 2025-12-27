using FoodDelivery.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;

namespace FoodDelivery.Infrastructure.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;

        // 👇 CẤU HÌNH QUAN TRỌNG:
        // 1. Dùng HTTP (không S) và Port 5292 cho khớp với máy bạn.
        // 2. _cancelUrl TRỎ VỀ CÙNG LINK với _returnUrl để Controller xử lý logic Hủy/Hoàn kho.
        private readonly string _returnUrl = "http://localhost:5292/api/orders/payos-return";
        private readonly string _cancelUrl = "http://localhost:5292/api/orders/payos-return";

        public PayOSService(IConfiguration configuration)
        {
            string clientId = configuration["PayOS:ClientId"]!;
            string apiKey = configuration["PayOS:ApiKey"]!;
            string checksumKey = configuration["PayOS:ChecksumKey"]!;

            // Khởi tạo thư viện
            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        public async Task<CreatePaymentResult> CreatePaymentLink(long orderCode, int amount, string description, List<ItemData> items)
        {
            // Tạo dữ liệu thanh toán
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: amount,
                description: description,
                items: items,
                cancelUrl: _cancelUrl, // Lúc này cancelUrl đã trỏ về đúng chỗ
                returnUrl: _returnUrl
            );

            // Gọi thư viện tạo link
            return await _payOS.createPaymentLink(paymentData);
        }

        // 👇 THÊM ĐOẠN CODE NÀY VÀO
        public WebhookData VerifyPaymentWebhookData(WebhookType webhookBody)
        {
            return _payOS.verifyPaymentWebhookData(webhookBody);
        }
    }
}