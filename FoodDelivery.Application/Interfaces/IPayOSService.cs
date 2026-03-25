<<<<<<< HEAD
﻿//using Net.payOS.Types;

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
=======
﻿using Net.payOS.Types;
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

namespace FoodDelivery.Application.Interfaces
{
    public interface IPayOSService
    {
<<<<<<< HEAD
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
=======
        // Hàm tạo link thanh toán
        Task<CreatePaymentResult> CreatePaymentLink(long orderCode, int amount, string description, List<ItemData> items);
        WebhookData VerifyPaymentWebhookData(WebhookType webhookBody);
    }
}
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
