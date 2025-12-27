//using System.Security.Claims;
//using FoodDelivery.API.DTOs;
//using FoodDelivery.API.Hubs;
//using FoodDelivery.Domain.Entities;
//using FoodDelivery.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;

//namespace FoodDelivery.API.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//[Authorize] // Mặc định phải đăng nhập mới vào được Controller này
//public class OrdersController(AppDbContext context, IHubContext<OrderHub> hubContext) : ControllerBase
//{
//    // ==========================================
//    // 1. USER: TẠO ĐƠN HÀNG
//    // ==========================================
//    [HttpPost("checkout")]
//    public async Task<IActionResult> Checkout([FromBody] CheckoutDto request)
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//        if (string.IsNullOrEmpty(userId)) return Unauthorized();

//        // Validate giỏ hàng
//        var cartItems = await context.CartItems
//            .Include(c => c.Product)
//            .Where(c => c.UserId == userId)
//            .ToListAsync();

//        if (cartItems.Count == 0) return BadRequest("Giỏ hàng trống!");

//        // Lấy tên User để thông báo
//        var user = await context.Users.FindAsync(userId);
//        string userName = user?.FullName ?? user?.UserName ?? "Khách";

//        // Tính tiền & Tạo đơn
//        decimal totalAmount = cartItems.Sum(c => c.Quantity * c.Product!.Price);
//        var order = new Order
//        {
//            UserId = userId,
//            OrderDate = DateTime.UtcNow,
//            Status = "Pending",
//            DeliveryAddress = request.DeliveryAddress,
//            TotalAmount = totalAmount,
//            OrderDetails = [.. cartItems.Select(c => new OrderDetail
//            {
//                ProductId = c.ProductId,
//                Quantity = c.Quantity,
//                UnitPrice = c.Product!.Price
//            })]
//        };

//        context.Orders.Add(order);
//        context.CartItems.RemoveRange(cartItems); // Xóa giỏ
//        await context.SaveChangesAsync();

//        // 🔥 GỬI SIGNALR CHO ADMIN
//        string thongBao = $"🔔 Đơn mới #{order.Id} - {userName} - {totalAmount:N0}đ";
//        await hubContext.Clients.All.SendAsync("ReceiveOrderNotification", thongBao);

//        return Ok(new { message = "Đặt hàng thành công!", orderId = order.Id });
//    }

//    // ==========================================
//    // 2. USER: XEM LỊCH SỬ CỦA MÌNH
//    // ==========================================
//    [HttpGet("my-history")]
//    public async Task<IActionResult> GetMyOrders()
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//        var orders = await context.Orders
//            .Where(o => o.UserId == userId)
//            .OrderByDescending(o => o.OrderDate)
//            .Select(o => new
//            {
//                o.Id,
//                o.OrderDate,
//                o.Status,
//                o.TotalAmount,
//                ItemCount = o.OrderDetails.Count
//            })
//            .ToListAsync();

//        return Ok(orders);
//    }

//    // ==========================================
//    // 3. ADMIN: XEM TẤT CẢ ĐƠN
//    // ==========================================
//    [HttpGet("all-orders")] // Đặt tên route rõ ràng
//    [Authorize(Roles = "Admin")] // 👈 Chỉ Admin mới vào được hàm này
//    public async Task<IActionResult> GetAllOrders()
//    {
//        var orders = await context.Orders
//            .OrderByDescending(o => o.OrderDate)
//            .Select(o => new
//            {
//                o.Id,
//                // Lấy tên khách hàng an toàn (tránh null)
//                CustomerName = o.User != null ? (o.User.FullName ?? o.User.UserName) : "Khách vãng lai",
//                CustomerEmail = o.User != null ? o.User.Email : "",
//                o.OrderDate,
//                o.Status,
//                o.TotalAmount,
//                o.DeliveryAddress
//            })
//            .ToListAsync();

//        return Ok(orders);
//    }

//    // ==========================================
//    // 4. ADMIN: CẬP NHẬT TRẠNG THÁI (CÓ SIGNALR)
//    // ==========================================
//    [HttpPut("update-status")]
//    [Authorize(Roles = "Admin")] // 👈 Chỉ Admin mới được duyệt
//    public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
//    {
//        // Validate trạng thái hợp lệ
//        var validStatuses = new[] { "Pending", "Cooking", "Shipping", "Completed", "Cancelled" };
//        if (!validStatuses.Contains(newStatus))
//        {
//            return BadRequest($"Trạng thái không hợp lệ! (Chỉ nhận: {string.Join(", ", validStatuses)})");
//        }

//        var order = await context.Orders.FindAsync(orderId);
//        if (order == null) return NotFound("Không tìm thấy đơn hàng!");

//        string oldStatus = order.Status;
//        order.Status = newStatus;
//        await context.SaveChangesAsync();

//        // 🔥 GỬI SIGNALR BÁO CHO KHÁCH
//        string message = $"Đơn hàng #{orderId} đã chuyển sang trạng thái: {newStatus}";
//        await hubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", message);

//        return Ok(new { message = "Cập nhật thành công!", orderId, oldStatus, newStatus });
//    }
//}