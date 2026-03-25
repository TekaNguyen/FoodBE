using Microsoft.AspNetCore.Http;

namespace FoodDelivery.API.DTOs // Namespace theo đúng thư mục
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
<<<<<<< HEAD
        public List<IFormFile>? ImageFiles { get; set; }
        // Thêm tùy chọn này nếu muốn xóa hết ảnh cũ để up bộ mới (tùy nhu cầu)
        public bool? ReplaceOldImages { get; set; } = false;
=======
        public IFormFile? ImageFile { get; set; }
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    }
}