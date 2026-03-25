using FoodDelivery.Application.Interfaces;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Microsoft.AspNetCore.SignalR; // 👈 Nhớ thêm dòng này
using FoodDelivery.API.Hubs;        // 👈 Nhớ thêm dòng này

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController(
        AppDbContext context,
        IPayOSService payOSService,
        IHubContext<OrderHub> hubContext // 👈 Inject thêm Hub vào đây
        ) : ControllerBase
    {
        [HttpPost("payos")]
        public async Task<IActionResult> HandlePayOSWebhook([FromBody] WebhookType body)
        {
            try
            {
                // ---------------------------------------------------------
                // 🛑 CHẾ ĐỘ TEST (FAKE DATA)
                // ---------------------------------------------------------

                // 1. Comment dòng xác thực chuẩn này lại:
                WebhookData data = payOSService.VerifyPaymentWebhookData(body);

                // 2. Thêm dòng này để lấy dữ liệu trực tiếp (Bỏ qua kiểm tra chữ ký):
               // WebhookData data = body.data;

                // ---------------------------------------------------------

                // 2. Lấy thông tin đơn hàng
                int orderId = (int)data.orderCode;
                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return Ok(new { message = "Order not found" });

                // 3. Cập nhật trạng thái
                if (order.PaymentStatus != "Paid")
                {
                    order.PaymentStatus = "Paid";
                    order.Status = "Confirmed";
                    order.PaymentDate = DateTime.UtcNow;

                    await context.SaveChangesAsync();

                    Console.WriteLine($"✅ Webhook: Đơn #{orderId} thanh toán thành công!");

                    // 👇 4. BẮN SIGNALR (Code này quan trọng để hiện thông báo)
                    await hubContext.Clients.Group($"Order-{orderId}")
                        .SendAsync("ReceivePaymentStatus", new
                        {
                            OrderId = orderId,
                            Status = "Paid",
                            Message = "Thanh toán thành công! (Test Mode)"
                        });
                }

                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Webhook Error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}