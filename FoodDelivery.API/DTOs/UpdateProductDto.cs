using Microsoft.AspNetCore.Http; // Để dùng IFormFile

namespace FoodDelivery.API.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }

    // 👇 Đừng quên nhân vật chính của chúng ta hôm nay
    public int? StockQuantity { get; set; }

    public IFormFile? ImageFile { get; set; }
}