using Microsoft.AspNetCore.Http;

namespace FoodDelivery.API.DTOs // Namespace theo đúng thư mục
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        // Thêm tùy chọn này nếu muốn xóa hết ảnh cũ để up bộ mới (tùy nhu cầu)
        public bool? ReplaceOldImages { get; set; } = false;
    }
}