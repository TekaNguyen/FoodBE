using FoodDelivery.API.DTOs;
using FoodDelivery.API.Hubs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Thêm thư viện này
using Net.payOS.Types;
using System.Security.Claims;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(
        AppDbContext context,
        IHubContext<OrderHub> hubContext,
        IPayOSService payOSService,
        IMemoryCache cache // Thêm IMemoryCache vào Primary Constructor
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

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            // 1. XÁC ĐỊNH NGƯỜI MUA
            var (userId, sessionId) = GetUserIdentity();
            if (userId == null && sessionId == null)
                return BadRequest("Không xác định được người mua.");

            // ==========================================
            // 🛡️ BỔ SUNG: THROTTLING (CHỐNG SPAM ĐẶT ĐƠN)
            // ==========================================
            string throttleKey = $"throttle_order_{userId ?? sessionId}";
            if (cache.TryGetValue(throttleKey, out _))
            {
                return StatusCode(429, "Thao tác quá nhanh. Vui lòng đợi 30 giây giữa các lần đặt đơn.");
            }

            var query = context.CartItems
                .Include(c => c.Product)
                .Include(c => c.SelectedOptions).ThenInclude(co => co.ProductOption)
                .AsQueryable();

            if (userId != null) query = query.Where(c => c.UserId == userId);
            else query = query.Where(c => c.SessionId == sessionId);

            var cartItems = await query.ToListAsync();
            if (cartItems.Count == 0) return BadRequest("Giỏ hàng đang trống!");

            // 2. CHECK KHO & TÍNH TIỀN
            decimal realSubTotal = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in cartItems)
            {
                if (item.Product == null) continue;
                if (item.Product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Món '{item.Product.Name}' chỉ còn {item.Product.StockQuantity} suất.");
                }

                item.Product.StockQuantity -= item.Quantity;

                decimal basePrice = item.Product.Price;
                decimal optionsPrice = item.SelectedOptions.Sum(o => o.ProductOption?.PriceModifier ?? 0);
                decimal finalUnitPrice = basePrice + optionsPrice;

                List<string> optionNames = [.. item.SelectedOptions
                    .Select(o => o.ProductOption?.Name)
                    .Where(n => n != null)!];
                string optionsSummary = string.Join(", ", optionNames);

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = finalUnitPrice,
                    OptionsSummary = optionsSummary
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

            // 4. COUPON
            decimal discountAmount = 0;
            string? usedCouponCode = null;

            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive);
                if (coupon == null) return BadRequest("Mã giảm giá không tồn tại.");
                if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate) return BadRequest("Mã đã hết hạn.");
                if (coupon.UsedCount >= coupon.UsageLimit) return BadRequest("Mã đã hết lượt dùng.");
                if (realSubTotal < coupon.MinOrderValue) return BadRequest($"Đơn chưa đủ {coupon.MinOrderValue:N0}đ để dùng mã này.");

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

            // 5. TỔNG TIỀN
            decimal finalTotal = realSubTotal + shippingFee - discountAmount;
            if (finalTotal < 0) finalTotal = 0;

            if (request.PaymentMethod == "PAYOS" && finalTotal < 2000 && finalTotal > 0)
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
                context.CartItems.RemoveRange(cartItems);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ==========================================
                // 🛡️ THIẾT LẬP KHÓA THROTTLE SAU KHI THÀNH CÔNG
                // ==========================================
                cache.Set(throttleKey, true, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi xử lý đơn hàng: {ex.Message}");
            }

            // 7. NẾU CHỌN PAYOS
            if (string.Equals(order.PaymentMethod, "PAYOS", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    List<ItemData> payosItems = [];
                    if (discountAmount > 0)
                    {
                        payosItems.Add(new ItemData($"Thanh toan don hang #{order.Id} (Da giam gia)", 1, (int)finalTotal));
                    }
                    else
                    {
                        payosItems = [.. order.OrderDetails.Select(x => new ItemData(x.ProductName, (int)x.Quantity, (int)x.UnitPrice))];
                        if (shippingFee > 0) payosItems.Add(new ItemData("Phi van chuyen", 1, (int)shippingFee));
                    }

                    var result = await payOSService.CreatePaymentLink(order.Id, (int)finalTotal, $"Thanh toan don {order.Id}", payosItems);

                    return Ok(new { orderId = order.Id, paymentUrl = result.checkoutUrl, message = "Chuyển hướng thanh toán PayOS" });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = "Lỗi tạo link PayOS", error = ex.Message });
                }
            }

            // 8. NẾU LÀ COD: BÁO CHO BẾP NGAY LẬP TỨC
            var newOrderData = new
            {
                order.Id,
                order.TotalAmount,
                order.FullName,
                Items = order.OrderDetails.Select(d => $"{d.Quantity}x {d.ProductName}").ToList(),
                Type = "COD"
            };

            await hubContext.Clients.Groups("Kitchen", "AdminOrders").SendAsync("ReceiveNewOrder", newOrderData);

            return Ok(new { orderId = order.Id, totalAmount = finalTotal, discountAmount, message = "Đặt hàng thành công (COD)" });
        }

        [HttpGet("payos-return")]
        public async Task<IActionResult> PayOSReturn(
            [FromQuery] string code,
            [FromQuery] bool cancel,
            [FromQuery] string status,
            [FromQuery] long orderCode)
        {
            var order = await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == (int)orderCode);
            if (order == null) return Content("<h1>❌ Lỗi: Không tìm thấy đơn hàng</h1>", "text/html");

            if (code == "00" && status == "PAID" && !cancel)
            {
                if (order.PaymentStatus != "Paid")
                {
                    order.PaymentStatus = "Paid";
                    order.Status = "Cooking";
                    await context.SaveChangesAsync();

                    await hubContext.Clients.Group($"Order-{orderCode}")
                        .SendAsync("ReceivePaymentStatus", new { OrderId = orderCode, Status = "Paid", Message = "Thanh toán thành công!" });

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
                return Content("<h1>✅ THANH TOÁN THÀNH CÔNG!</h1><p>Bạn có thể đóng tab này.</p>", "text/html");
            }

            if (cancel)
            {
                order.Status = "Cancelled";
                order.PaymentStatus = "Cancelled";

                var orderDetails = await context.OrderDetails.Where(od => od.OrderId == (int)orderCode).ToListAsync();
                foreach (var item in orderDetails)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null) product.StockQuantity += item.Quantity;
                }
                await context.SaveChangesAsync();
                return Content("<h1>❌ THANH TOÁN ĐÃ BỊ HỦY</h1>", "text/html");
            }

            return Content("<h1>❌ Lỗi không xác định</h1>", "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            var (userId, _) = GetUserIdentity();

            var query = context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (userId != null) query = query.Where(o => o.UserId == userId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

            var totalItems = await query.CountAsync();
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
                 ItemCount = o.OrderDetails.Count,
                 FirstItemName = o.OrderDetails.FirstOrDefault() != null
                                 ? o.OrderDetails.FirstOrDefault()!.ProductName
                                 : "Không có món",
                 ReceiverName = o.FullName
             })
             .ToListAsync();

            return Ok(new { Total = totalItems, Page = page, Data = orders });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng");
            return Ok(order);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Cooking", "Delivering", "Completed", "Cancelled" };
            if (!validStatuses.Contains(newStatus))
                return BadRequest("Trạng thái không hợp lệ.");

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

            await hubContext.Clients.Group($"Order-{id}")
                .SendAsync("ReceiveStatusUpdate", new { OrderId = id, Status = newStatus, Message = $"Đơn hàng #{id} chuyển sang: {newStatus}" });

            await hubContext.Clients.Groups("AdminOrders", "Kitchen")
                .SendAsync("ReloadOrderList", id);

            return Ok(new { message = $"Đã chuyển trạng thái từ {oldStatus} -> {newStatus}" });
        }
    }
}



//using FoodDelivery.API.DTOs;
//using FoodDelivery.API.Hubs;
//using FoodDelivery.Application.Interfaces;
//using FoodDelivery.Domain.Entities;
//using FoodDelivery.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Net.payOS.Types;
//using System.Security.Claims;

//namespace FoodDelivery.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class OrdersController(
//        AppDbContext context,
//        IHubContext<OrderHub> hubContext,
//        IPayOSService payOSService
//        ) : ControllerBase
//    {
//        // ====================================
//        // 🛠️ HÀM PHỤ: Xác định User hay Guest
//        // ====================================
//        private (string? userId, string? sessionId) GetUserIdentity()
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (!string.IsNullOrEmpty(userId)) return (userId, null);

//            if (Request.Headers.TryGetValue("X-Guest-ID", out var guestId))
//                return (null, guestId.ToString());

//            return (null, null);
//        }

//        // ==========================================
//        // 🛒 CHECKOUT (Hỗ trợ Guest + User + PAYOS + COD)
//        // ==========================================
//        [HttpPost("checkout")]
//        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
//        {
//            // 1. XÁC ĐỊNH NGƯỜI MUA
//            var (userId, sessionId) = GetUserIdentity();
//            if (userId == null && sessionId == null)
//                return BadRequest("Không xác định được người mua.");

//            var query = context.CartItems
//                .Include(c => c.Product)
//                .Include(c => c.SelectedOptions).ThenInclude(co => co.ProductOption)
//                .AsQueryable();

//            if (userId != null) query = query.Where(c => c.UserId == userId);
//            else query = query.Where(c => c.SessionId == sessionId);

//            var cartItems = await query.ToListAsync();
//            if (cartItems.Count == 0) return BadRequest("Giỏ hàng đang trống!");

//            // 2. CHECK KHO & TÍNH TIỀN
//            decimal realSubTotal = 0;
//            var orderDetails = new List<OrderDetail>();

//            foreach (var item in cartItems)
//            {
//                if (item.Product == null) continue;
//                if (item.Product.StockQuantity < item.Quantity)
//                {
//                    return BadRequest($"Món '{item.Product.Name}' chỉ còn {item.Product.StockQuantity} suất.");
//                }

//                item.Product.StockQuantity -= item.Quantity; // Trừ kho

//                decimal basePrice = item.Product.Price;
//                decimal optionsPrice = item.SelectedOptions.Sum(o => o.ProductOption?.PriceModifier ?? 0);
//                decimal finalUnitPrice = basePrice + optionsPrice;

//                List<string> optionNames = [.. item.SelectedOptions
//                    .Select(o => o.ProductOption?.Name)
//                    .Where(n => n != null)!];
//                string optionsSummary = string.Join(", ", optionNames);

//                orderDetails.Add(new OrderDetail
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.Product.Name,
//                    Quantity = item.Quantity,
//                    UnitPrice = finalUnitPrice,
//                    OptionsSummary = optionsSummary
//                });

//                realSubTotal += finalUnitPrice * item.Quantity;
//            }

//            // 3. TÍNH SHIP
//            decimal shippingFee = 0;
//            var settingShip = await context.Settings.FirstOrDefaultAsync(s => s.Key == "shipping_fee");
//            if (settingShip != null && decimal.TryParse(settingShip.Value, out decimal parsedFee))
//            {
//                shippingFee = parsedFee;
//            }

//            // 4. COUPON
//            decimal discountAmount = 0;
//            string? usedCouponCode = null;

//            if (!string.IsNullOrEmpty(request.CouponCode))
//            {
//                var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive);
//                if (coupon == null) return BadRequest("Mã giảm giá không tồn tại.");
//                if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate) return BadRequest("Mã đã hết hạn.");
//                if (coupon.UsedCount >= coupon.UsageLimit) return BadRequest("Mã đã hết lượt dùng.");
//                if (realSubTotal < coupon.MinOrderValue) return BadRequest($"Đơn chưa đủ {coupon.MinOrderValue:N0}đ để dùng mã này.");

//                if (coupon.DiscountType == "FIXED") discountAmount = coupon.DiscountValue;
//                else
//                {
//                    discountAmount = realSubTotal * (coupon.DiscountValue / 100);
//                    if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
//                        discountAmount = coupon.MaxDiscountAmount.Value;
//                }

//                if (discountAmount > realSubTotal) discountAmount = realSubTotal;
//                usedCouponCode = coupon.Code;
//                coupon.UsedCount++;
//            }

//            // 5. TỔNG TIỀN
//            decimal finalTotal = realSubTotal + shippingFee - discountAmount;
//            if (finalTotal < 0) finalTotal = 0;

//            if (request.PaymentMethod == "PAYOS" && finalTotal < 2000 && finalTotal > 0)
//            {
//                return BadRequest("Số tiền thanh toán tối thiểu qua PayOS là 2.000đ");
//            }

//            // 6. LƯU ORDER VÀO DB
//            var order = new Order
//            {
//                UserId = userId,
//                FullName = request.ReceiverName,
//                PhoneNumber = request.ReceiverPhone,
//                DeliveryAddress = request.DeliveryAddress,
//                Note = request.Note,
//                OrderDate = DateTime.UtcNow,
//                Status = "Pending",
//                PaymentMethod = request.PaymentMethod,
//                PaymentStatus = "Pending",
//                ShippingFee = shippingFee,
//                TotalAmount = finalTotal,
//                CouponCode = usedCouponCode,
//                DiscountAmount = discountAmount,
//                OrderDetails = orderDetails
//            };

//            using var transaction = context.Database.BeginTransaction();
//            try
//            {
//                context.Orders.Add(order);
//                context.CartItems.RemoveRange(cartItems);
//                await context.SaveChangesAsync();
//                await transaction.CommitAsync();
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return StatusCode(500, $"Lỗi xử lý đơn hàng: {ex.Message}");
//            }

//            // =========================================================
//            // 👇 LOGIC REALTIME & THANH TOÁN (ĐÃ SỬA CHUẨN) 👇
//            // =========================================================

//            // 7. NẾU CHỌN PAYOS
//            if (string.Equals(order.PaymentMethod, "PAYOS", StringComparison.OrdinalIgnoreCase))
//            {
//                try
//                {
//                    List<ItemData> payosItems = [];
//                    if (discountAmount > 0)
//                    {
//                        payosItems.Add(new ItemData($"Thanh toan don hang #{order.Id} (Da giam gia)", 1, (int)finalTotal));
//                    }
//                    else
//                    {
//                        payosItems = [.. order.OrderDetails.Select(x => new ItemData(x.ProductName, (int)x.Quantity, (int)x.UnitPrice))];
//                        if (shippingFee > 0) payosItems.Add(new ItemData("Phi van chuyen", 1, (int)shippingFee));
//                    }

//                    var result = await payOSService.CreatePaymentLink(order.Id, (int)finalTotal, $"Thanh toan don {order.Id}", payosItems);

//                    return Ok(new { orderId = order.Id, paymentUrl = result.checkoutUrl, message = "Chuyển hướng thanh toán PayOS" });
//                }
//                catch (Exception ex)
//                {
//                    return BadRequest(new { message = "Lỗi tạo link PayOS", error = ex.Message });
//                }
//            }

//            // 8. NẾU LÀ COD: BÁO CHO BẾP NGAY LẬP TỨC
//            var newOrderData = new
//            {
//                order.Id,
//                order.TotalAmount,
//                order.FullName,
//                Items = order.OrderDetails.Select(d => $"{d.Quantity}x {d.ProductName}").ToList(),
//                Type = "COD"
//            };

//            // Bắn tín hiệu vào kênh "Kitchen" và "AdminOrders"
//            await hubContext.Clients.Groups("Kitchen", "AdminOrders")
//                .SendAsync("ReceiveNewOrder", newOrderData);

//            return Ok(new { orderId = order.Id, totalAmount = finalTotal, discountAmount, message = "Đặt hàng thành công (COD)" });
//        }

//        // ==========================================
//        // 🔄 PAYOS RETURN (ĐÃ BỔ SUNG NOTIFICATION)
//        // ==========================================
//        [HttpGet("payos-return")]
//        public async Task<IActionResult> PayOSReturn(
//            [FromQuery] string code,
//            [FromQuery] bool cancel,
//            [FromQuery] string status,
//            [FromQuery] long orderCode)
//        {
//            var order = await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == (int)orderCode);
//            if (order == null) return Content("<h1>❌ Lỗi: Không tìm thấy đơn hàng</h1>", "text/html");

//            if (code == "00" && status == "PAID" && !cancel)
//            {
//                if (order.PaymentStatus != "Paid")
//                {
//                    order.PaymentStatus = "Paid";
//                    order.Status = "Cooking"; // Chuyển thẳng sang nấu
//                    await context.SaveChangesAsync();

//                    // 1. Báo cho KHÁCH HÀNG (User đang xem trang thanh toán)
//                    await hubContext.Clients.Group($"Order-{orderCode}")
//                        .SendAsync("ReceivePaymentStatus", new { OrderId = orderCode, Status = "Paid", Message = "Thanh toán thành công!" });

//                    // 2. BÁO CHO BẾP & ADMIN "CÓ ĐƠN MỚI ĐÃ TRẢ TIỀN"
//                    var newOrderData = new
//                    {
//                        order.Id,
//                        order.TotalAmount,
//                        order.FullName,
//                        Items = order.OrderDetails.Select(d => $"{d.Quantity}x {d.ProductName}").ToList(),
//                        Type = "PAYOS_PAID"
//                    };

//                    await hubContext.Clients.Groups("Kitchen", "AdminOrders")
//                        .SendAsync("ReceiveNewOrder", newOrderData);
//                }
//                return Content("<h1>✅ THANH TOÁN THÀNH CÔNG!</h1><p>Bạn có thể đóng tab này.</p>", "text/html");
//            }

//            if (cancel)
//            {
//                order.Status = "Cancelled";
//                order.PaymentStatus = "Cancelled";

//                // Hoàn kho
//                var orderDetails = await context.OrderDetails.Where(od => od.OrderId == orderCode).ToListAsync();
//                foreach (var item in orderDetails)
//                {
//                    var product = await context.Products.FindAsync(item.ProductId);
//                    if (product != null) product.StockQuantity += item.Quantity;
//                }
//                await context.SaveChangesAsync();
//                return Content("<h1>❌ THANH TOÁN ĐÃ BỊ HỦY</h1>", "text/html");
//            }

//            return Content("<h1>❌ Lỗi không xác định</h1>", "text/html");
//        }

//        // ==========================================
//        // ADMIN & USER: GET ORDERS
//        // ==========================================
//        [HttpGet]
//        public async Task<IActionResult> GetOrders(
//            [FromQuery] int page = 1,
//            [FromQuery] int pageSize = 10,
//            [FromQuery] string? status = null)
//        {
//            var (userId, sessionId) = GetUserIdentity();

//            var query = context.Orders
//                .Include(o => o.OrderDetails)
//                .OrderByDescending(o => o.OrderDate)
//                .AsQueryable();

//            if (userId != null) query = query.Where(o => o.UserId == userId);

//            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

//            var totalItems = await query.CountAsync();
//            var orders = await query
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(o => new
//             {
//                 o.Id,
//                 o.Status,
//                 o.PaymentStatus,
//                 o.TotalAmount,
//                 o.OrderDate,
//                 o.PaymentMethod,
//                 ItemCount = o.OrderDetails.Count,
//                 FirstItemName = o.OrderDetails.FirstOrDefault() != null
//                                                 ? o.OrderDetails.FirstOrDefault()!.ProductName
//                                                 : "Không có món",
//                 ReceiverName = o.FullName
//             })
//             .ToListAsync();

//            return Ok(new { Total = totalItems, Page = page, Data = orders });
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetOrderById(int id)
//        {
//            var order = await context.Orders
//                .Include(o => o.OrderDetails)
//                .FirstOrDefaultAsync(o => o.Id == id);

//            if (order == null) return NotFound("Không tìm thấy đơn hàng");
//            return Ok(order);
//        }

//        // ==========================================
//        // CẬP NHẬT TRẠNG THÁI (ĐÃ CÓ SIGNALR)
//        // ==========================================
//        [HttpPut("{id}/status")]
//        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
//        {
//            var validStatuses = new[] { "Pending", "Confirmed", "Cooking", "Delivering", "Completed", "Cancelled" };
//            if (!validStatuses.Contains(newStatus))
//                return BadRequest("Trạng thái không hợp lệ.");

//            var order = await context.Orders.FindAsync(id);
//            if (order == null) return NotFound();

//            if (order.Status == "Cancelled" || order.Status == "Completed")
//                return BadRequest($"Đơn hàng đang ở '{order.Status}', không thể thay đổi.");

//            string oldStatus = order.Status;
//            order.Status = newStatus;

//            if (newStatus == "Completed" && order.PaymentMethod == "COD")
//            {
//                order.PaymentStatus = "Paid";
//                order.PaymentDate = DateTime.UtcNow;
//            }

//            await context.SaveChangesAsync();

//            // 1. Báo cho Khách
//            await hubContext.Clients.Group($"Order-{id}")
//                .SendAsync("ReceiveStatusUpdate", new { OrderId = id, Status = newStatus, Message = $"Đơn hàng #{id} chuyển sang: {newStatus}" });

//            // 2. Báo cho Admin/Kitchen reload
//            await hubContext.Clients.Groups("AdminOrders", "Kitchen")
//                .SendAsync("ReloadOrderList", id);

//            return Ok(new { message = $"Đã chuyển trạng thái từ {oldStatus} -> {newStatus}" });
//        }
//    }
//}


