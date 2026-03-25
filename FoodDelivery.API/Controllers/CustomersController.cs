using FoodDelivery.API.DTOs;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuestCustomerDto>>> GetCustomersFromOrders()
        {
            var query = context.Orders
                .AsNoTracking()
                .Where(o => !string.IsNullOrEmpty(o.PhoneNumber))
                // 👇 THAY ĐỔI QUAN TRỌNG Ở ĐÂY:
                // Gom nhóm theo CẢ Số điện thoại VÀ Tên khách hàng
                // Để "Nguyễn Phú Hào" và "L Cương" dù trùng số vẫn tách ra 2 dòng
                .GroupBy(o => new { o.PhoneNumber, o.FullName })
                .Select(g => new GuestCustomerDto
                {
                    // Lấy Key từ nhóm đã gom
                    PhoneNumber = g.Key.PhoneNumber,
                    FullName = g.Key.FullName,

                    // Địa chỉ lấy của đơn mới nhất trong nhóm đó
                    Address = g.OrderByDescending(o => o.OrderDate).First().DeliveryAddress,

                    // Thống kê
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    LastOrderDate = g.Max(o => o.OrderDate)
                });

            return await query.OrderByDescending(c => c.LastOrderDate).ToListAsync();
        }
    }
}