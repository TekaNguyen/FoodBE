//using Net.payOS.Types;

//namespace FoodDelivery.Application.Interfaces
//{
//    public interface IPayOSService
//    {
//        // Hàm tạo link thanh toán
//        Task<CreatePaymentResult> CreatePaymentLink(long orderCode, int amount, string description, List<ItemData> items);
//        WebhookData VerifyPaymentWebhookData(WebhookType webhookBody);
//    }
//}

using Net.payOS.Types;

namespace FoodDelivery.Application.Interfaces
{
    public interface IPayOSService
    {
        // 👇 PHẢI SỬA DÒNG NÀY ĐỂ KHỚP VỚI CLASS
        Task<CreatePaymentResult> CreatePaymentLink(
            long orderCode,
            int amount,
            string description,
            List<ItemData> items,
            string? returnUrl = null,
            string? cancelUrl = null
        );

        WebhookData VerifyPaymentWebhookData(WebhookType webhookBody);
    }
}