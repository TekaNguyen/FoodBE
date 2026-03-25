namespace FoodDelivery.API.DTOs;

public class CreateOrderDto
{
    // Thông tin người nhận hàng (Khách tự nhập)
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string? Note { get; set; }

    // Danh sách món ăn
    public List<CartItemDto> Items { get; set; } = [];
}
