using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.API.DTOs;
using FoodDelivery.Infrastructure.Persistence;

namespace FoodDelivery.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")] // 🔒 Chỉ Admin được xem số liệu
public class StatsController(AppDbContext context) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboardStats()
    {
        var today = DateTime.UtcNow.Date; // Lấy ngày hôm nay (giờ quốc tế)
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        // 1. Truy vấn Doanh thu & Số lượng đơn
        // Lưu ý: Chỉ tính tiền những đơn đã Hoàn thành (Completed)
        var revenueToday = await context.Orders
            .Where(o => o.OrderDate >= today && o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);

        var revenueThisMonth = await context.Orders
            .Where(o => o.OrderDate >= startOfMonth && o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);

        var totalOrders = await context.Orders.CountAsync();
        var completedOrders = await context.Orders.CountAsync(o => o.Status == "Completed");
        var cancelledOrders = await context.Orders.CountAsync(o => o.Status == "Cancelled");

        // 2. Truy vấn Top 5 Món bán chạy nhất (Phức tạp nhất này!)
        // Logic: Vào bảng OrderDetail -> Nhóm theo ProductId -> Tính tổng số lượng -> Sắp xếp giảm dần
        var topProducts = await context.OrderDetails
            .Include(od => od.Product) // Kèm thông tin sản phẩm
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                g.First().Product,
                TotalQuantity = g.Sum(od => od.Quantity),
                TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantity) // Sắp xếp bán chạy nhất lên đầu
            .Take(5) // Lấy top 5
            .Select(x => new TopProductDto
            {
                ProductName = x.Product!.Name,
                QuantitySold = x.TotalQuantity,
                TotalRevenue = x.TotalRevenue
            })
            .ToListAsync();

        // 3. Đóng gói trả về
        var dashboardData = new DashboardDto
        {
            RevenueToday = revenueToday,
            RevenueThisMonth = revenueThisMonth,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            CancelledOrders = cancelledOrders,
            TopProducts = topProducts
        };

        return Ok(dashboardData);
    }
}