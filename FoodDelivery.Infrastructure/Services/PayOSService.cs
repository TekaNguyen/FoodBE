//using FoodDelivery.Application.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Net.payOS;
//using Net.payOS.Types;

//namespace FoodDelivery.Infrastructure.Services
//{
//    public class PayOSService : IPayOSService
//    {
//        private readonly PayOS _payOS;

//        // 👇 CẤU HÌNH QUAN TRỌNG:
//        // 1. Dùng HTTP (không S) và Port 5292 cho khớp với máy bạn.
//        // 2. _cancelUrl TRỎ VỀ CÙNG LINK với _returnUrl để Controller xử lý logic Hủy/Hoàn kho.
//        private readonly string _returnUrl = "http://localhost:5292/api/orders/payos-return";
//        private readonly string _cancelUrl = "http://localhost:5292/api/orders/payos-return";

//        public PayOSService(IConfiguration configuration)
//        {
//            string clientId = configuration["PayOS:ClientId"]!;
//            string apiKey = configuration["PayOS:ApiKey"]!;
//            string checksumKey = configuration["PayOS:ChecksumKey"]!;

//            // Khởi tạo thư viện
//            _payOS = new PayOS(clientId, apiKey, checksumKey);
//        }

//        //public async Task<CreatePaymentResult> CreatePaymentLink(long orderCode, int amount, string description, List<ItemData> items)
//        //{
//        //    // Tạo dữ liệu thanh toán
//        //    var paymentData = new PaymentData(
//        //        orderCode: orderCode,
//        //        amount: amount,
//        //        description: description,
//        //        items: items,
//        //        cancelUrl: _cancelUrl, // Lúc này cancelUrl đã trỏ về đúng chỗ
//        //        returnUrl: _returnUrl
//        //    );

//        //    // Gọi thư viện tạo link
//        //    return await _payOS.createPaymentLink(paymentData);
//        //}

//        //// 👇 THÊM ĐOẠN CODE NÀY VÀO
//        //public WebhookData VerifyPaymentWebhookData(WebhookType webhookBody)
//        //{
//        //    return _payOS.verifyPaymentWebhookData(webhookBody);
//        //}
//        // Thêm 2 tham số cuối là returnUrl và cancelUrl (mặc định là null nếu không truyền)
//        public async Task<CreatePaymentResult> CreatePaymentLink(long orderCode, int amount, string description, List<ItemData> items, string? returnUrl = null, string? cancelUrl = null)
//        {
//            // Tạo dữ liệu thanh toán
//            var paymentData = new PaymentData(
//                orderCode: orderCode, // Đây sẽ là mã ngẫu nhiên (Ticks)
//                amount: amount,
//                description: description,
//                items: items,
//                // 👇 Logic mới: Nếu Controller truyền link vào thì dùng, không thì dùng mặc định trong config
//                cancelUrl: cancelUrl ?? _cancelUrl,
//                returnUrl: returnUrl ?? _returnUrl
//            );

//            // Gọi thư viện tạo link
//            return await _payOS.createPaymentLink(paymentData);
//        }

//        // 👇 ĐOẠN BẠN YÊU CẦU THÊM VÀO
//        public WebhookData VerifyPaymentWebhookData(WebhookType webhookBody)
//        {
//            return _payOS.verifyPaymentWebhookData(webhookBody);
//        }
//    }
//}

using FoodDelivery.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;

namespace FoodDelivery.Infrastructure.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;

        // 👇 CẤU HÌNH MẶC ĐỊNH (Sẽ được dùng nếu Controller không truyền link vào)
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

        // 👇 Hàm tạo link thanh toán (Hỗ trợ tùy chỉnh URL)
        public async Task<CreatePaymentResult> CreatePaymentLink(
            long orderCode,
            int amount,
            string description,
            List<ItemData> items,
            string? returnUrl = null,
            string? cancelUrl = null)
        {
            // Tạo dữ liệu thanh toán
            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: amount,
                description: description,
                items: items,
                // 👇 Logic ưu tiên: Nếu có link truyền vào thì dùng, không thì dùng mặc định
                cancelUrl: cancelUrl ?? _cancelUrl,
                returnUrl: returnUrl ?? _returnUrl
            );

            // Gọi thư viện tạo link
            return await _payOS.createPaymentLink(paymentData);
        }

        // 👇 Hàm xác thực Webhook (Dùng cho sau này nếu cần)
        public WebhookData VerifyPaymentWebhookData(WebhookType webhookBody)
        {
            return _payOS.verifyPaymentWebhookData(webhookBody);
        }
    }
}