using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Authorize(Roles = "Admin")]
public class DashboardController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet("stats")]
    public async Task<IActionResult> GetBasicStats()
    {
        // Tính toán các số liệu cơ bản
        var totalSales = await _context.Orders
            .Where(o => o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);

        var orderCount = await _context.Orders.CountAsync();
        var userCount = await _context.Users.CountAsync();
        var productCount = await _context.Products.CountAsync();

        // Lấy top sản phẩm bán chạy (Ví dụ đơn giản)
        var topProducts = await _context.Products
            .OrderByDescending(p => p.StockQuantity) // Tạm thời theo kho, sau này theo OrderDetails
            .Take(5)
            .Select(p => new { p.Name, p.Price })
            .ToListAsync();

        return Ok(new
        {
            TotalSales = totalSales,
            TotalOrders = orderCount,
            TotalUsers = userCount,
            TotalProducts = productCount,
            TopProducts = topProducts
        });
    }
}