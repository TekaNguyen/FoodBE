namespace FoodDelivery.API.DTOs;

public class DashboardDto
{
    // 1. Thống kê Doanh thu
    public decimal RevenueToday { get; set; }      // Doanh thu hôm nay
    public decimal RevenueThisMonth { get; set; }  // Doanh thu tháng này

    // 2. Thống kê Đơn hàng
    public int TotalOrders { get; set; }      // Tổng số đơn
    public int CompletedOrders { get; set; }  // Số đơn thành công
    public int CancelledOrders { get; set; }  // Số đơn hủy

    // 3. Top món ăn bán chạy
    public List<TopProductDto> TopProducts { get; set; } = [];
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}