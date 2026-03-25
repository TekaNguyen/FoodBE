using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

// 1. DTO Tạo mới (Thêm CategoryId vào trong ngoặc)
public record CreateProductDto(
    [Required(ErrorMessage = "Tên món không được để trống")] string Name,
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")] decimal Price,

    // 👇 THÊM VÀO ĐÂY (Bắt buộc chọn danh mục khi tạo)
    int CategoryId,

    string? Description = null,
    IFormFile? ImageFile = null
);
