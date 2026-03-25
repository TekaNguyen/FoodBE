using FoodDelivery.API.Hubs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Net.payOS.Types;
using System.Security.Claims;
using FoodDelivery.API.DTOs;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(
        AppDbContext context,
        IHubContext<OrderHub> hubContext,
        IPayOSService payOSService,
        IMemoryCache cache
        ) : ControllerBase
    {
        private (string? userId, string? sessionId) GetUserIdentity()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId)) return (userId, null);

            if (Request.Headers.TryGetValue("X-Guest-ID", out var guestId))
                return (null, guestId.ToString());

            return (null, null);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            // 1. XÁC ĐỊNH NGƯỜI MUA
            var (userId, sessionId) = GetUserIdentity();

            // 🛡️ THROTTLING (CHỐNG SPAM)
            string throttleKey = $"throttle_order_{userId ?? sessionId ?? "unknown"}";
            if (cache.TryGetValue(throttleKey, out _))
            {
                return StatusCode(429, "Thao tác quá nhanh. Vui lòng đợi 30 giây.");
            }

            if (request.Details == null || request.Details.Count == 0)
            {
                return BadRequest("Giỏ hàng rỗng (Vui lòng chọn món trước khi đặt)!");
            }

            // 2. CHECK KHO & TÍNH TIỀN
            decimal realSubTotal = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in request.Details)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                if (product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Món '{product.Name}' chỉ còn {product.StockQuantity} suất.");
                }

                // Trừ kho
                product.StockQuantity -= item.Quantity;

                decimal finalUnitPrice = product.Price;
                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = finalUnitPrice
                });

                realSubTotal += finalUnitPrice * item.Quantity;
            }

            // 3. TÍNH SHIP
            decimal shippingFee = 0;
            var settingShip = await context.Settings.FirstOrDefaultAsync(s => s.Key == "shipping_fee");
            if (settingShip != null && decimal.TryParse(settingShip.Value, out decimal parsedFee))
            {
                shippingFee = parsedFee;
            }

            // 4. COUPON (Logic giữ nguyên)
            decimal discountAmount = 0;
            string? usedCouponCode = null;
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive);
                if (coupon != null)
                {
                    bool isValid = true;
                    if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate) isValid = false;
                    if (coupon.UsedCount >= coupon.UsageLimit) isValid = false;
                    if (realSubTotal < coupon.MinOrderValue) isValid = false;

                    if (isValid)
                    {
                        if (coupon.DiscountType == "FIXED") discountAmount = coupon.DiscountValue;
                        else
                        {
                            discountAmount = realSubTotal * (coupon.DiscountValue / 100);
                            if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                                discountAmount = coupon.MaxDiscountAmount.Value;
                        }

                        if (discountAmount > realSubTotal) discountAmount = realSubTotal;
                        usedCouponCode = coupon.Code;
                        coupon.UsedCount++;
                    }
                }
            }

            // 5. TỔNG TIỀN
            decimal finalTotal = realSubTotal + shippingFee - discountAmount;
            if (finalTotal < 0) finalTotal = 0;

            if (request.PaymentMethod == "BANK_TRANSFER" && finalTotal < 2000 && finalTotal > 0)
            {
                return BadRequest("Số tiền thanh toán tối thiểu qua PayOS là 2.000đ");
            }

            // 6. LƯU ORDER VÀO DB
            var order = new Order
            {
                UserId = userId,
                FullName = request.ReceiverName,
                PhoneNumber = request.ReceiverPhone,
                DeliveryAddress = request.DeliveryAddress,
                Note = request.Note,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending",
                ShippingFee = shippingFee,
                TotalAmount = finalTotal,
                CouponCode = usedCouponCode,
                DiscountAmount = discountAmount,
                OrderDetails = orderDetails
            };

            using var transaction = context.Database.BeginTransaction();
            try
            {
                context.Orders.Add(order);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                cache.Set(throttleKey, true, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi xử lý đơn hàng: {ex.Message}");
            }

            // 7. TẠO LINK PAYOS
            if (string.Equals(order.PaymentMethod, "BANK_TRANSFER", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    long payOsCode = long.Parse(DateTime.Now.ToString("yyMMddHHmmss"));

                    List<ItemData> payosItems = [];
                    payosItems.Add(new ItemData($"Don hang #{order.Id}", 1, (int)finalTotal));

                    string returnUrl = $"http://localhost:5292/api/orders/payos-return?myOrderId={order.Id}";
                    string cancelUrl = "http://localhost:3000?error=cancelled";

                    var result = await payOSService.CreatePaymentLink(
                        payOsCode,
                        (int)finalTotal,
                        $"Don hang {order.Id}",
                        payosItems,
                        returnUrl,
                        cancelUrl
                    );

                    return Ok(new { orderId = order.Id, url = result.checkoutUrl, message = "Chuyển hướng thanh toán PayOS" });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi PayOS: " + ex.Message);
                    return BadRequest(new { message = "Lỗi tạo link PayOS", error = ex.Message });
                }
            }

            // 8. BÁO REALTIME (SIGNALR)
            var newOrderData = new
            {
                order.Id,
                order.TotalAmount,
                order.FullName,
                Items = order.OrderDetails.Select(d => $"{d.Quantity}x {d.ProductName}").ToList(),
                Type = "COD"
            };

            await hubContext.Clients.Groups("Kitchen", "AdminOrders").SendAsync("ReceiveNewOrder", newOrderData);

            return Ok(new { orderId = order.Id, totalAmount = finalTotal, message = "Đặt hàng thành công (COD)" });
        }

        // ==========================================
        // ✅ ĐÃ SỬA: ÁP DỤNG PHÂN TRANG ĐỂ DÙNG BIẾN page, pageSize
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate);

            // Đếm tổng số đơn để trả về metadata (nếu cần)
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang (Fix warning unused param)
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.PaymentStatus,
                    o.TotalAmount,
                    o.OrderDate,
                    o.PaymentMethod,
                    ItemCount = o.OrderDetails.Sum(x => x.Quantity),
                    FirstItemName = o.OrderDetails.FirstOrDefault() != null ? o.OrderDetails.FirstOrDefault()!.ProductName : "Món ăn",
                    ReceiverName = o.FullName
                })
                .ToListAsync();

            return Ok(new
            {
                data = orders,
                meta = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        }

        [HttpGet("payos-return")]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string code,
            [FromQuery] bool cancel,
            [FromQuery] string status,
            [FromQuery] long orderCode, // Sẽ log cái này để fix warning
            [FromQuery] int myOrderId)
        {
            // Log lại mã giao dịch của PayOS để tra cứu nếu cần (Fix warning unused param)
            Console.WriteLine($"[PAYOS RETURN] OrderCode: {orderCode} | Status: {status} | Code: {code}");

            var order = await context.Orders
                                    .Include(o => o.OrderDetails)
                                    .FirstOrDefaultAsync(o => o.Id == myOrderId);

            if (order == null) return Redirect("http://localhost:3000?error=order_not_found");

            // 1. THANH TOÁN THÀNH CÔNG
            if (code == "00" && status == "PAID" && !cancel)
            {
                if (order.PaymentStatus != "Paid")
                {
                    order.PaymentStatus = "Paid";
                    order.Status = "Cooking"; // Chuyển sang đang nấu
                    await context.SaveChangesAsync();

                    // Báo Realtime
                    var newOrderData = new
                    {
                        order.Id,
                        order.TotalAmount,
                        order.FullName,
                        Items = order.OrderDetails.Select(d => $"{d.Quantity}x {d.ProductName}").ToList(),
                        Type = "PAYOS_PAID"
                    };
                    await hubContext.Clients.Groups("Kitchen", "AdminOrders").SendAsync("ReceiveNewOrder", newOrderData);
                }

                // Redirect về Frontend
                return Redirect($"http://localhost:3000/payment-success?orderId={myOrderId}");
            }

            // 2. HỦY
            if (cancel)
            {
                order.Status = "Cancelled";
                order.PaymentStatus = "Cancelled";
                // Hoàn lại kho
                foreach (var item in order.OrderDetails)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null) product.StockQuantity += item.Quantity;
                }
                await context.SaveChangesAsync();

                return Redirect($"http://localhost:3000?error=payment_cancelled");
            }

            return Redirect($"http://localhost:3000?error=payment_failed");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            return Ok(order);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Cooking", "Delivering", "Completed", "Cancelled" };
            if (!validStatuses.Contains(newStatus)) return BadRequest("Trạng thái không hợp lệ.");

            var order = await context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == "Cancelled" || order.Status == "Completed")
                return BadRequest($"Đơn hàng đang ở '{order.Status}', không thể thay đổi.");

            string oldStatus = order.Status;
            order.Status = newStatus;

            if (newStatus == "Completed" && order.PaymentMethod == "COD")
            {
                order.PaymentStatus = "Paid";
                order.PaymentDate = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            await hubContext.Clients.Group($"Order-{id}").SendAsync("ReceiveStatusUpdate", new { OrderId = id, Status = newStatus, Message = $"Đơn hàng #{id} chuyển sang: {newStatus}" });
            await hubContext.Clients.Groups("AdminOrders", "Kitchen").SendAsync("ReloadOrderList", id);

            return Ok(new { message = $"Đã chuyển trạng thái từ {oldStatus} -> {newStatus}" });
        }
    }
}