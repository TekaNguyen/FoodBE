using Microsoft.AspNetCore.Http;

namespace FoodDelivery.API.DTOs // Namespace theo đúng thư mục
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}