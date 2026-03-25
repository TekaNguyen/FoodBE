using Microsoft.AspNetCore.Http;

namespace FoodDelivery.API.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }
    public int? StockQuantity { get; set; }
    public List<IFormFile>? ImageFiles { get; set; }
}