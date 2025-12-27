using CsvHelper.Configuration.Attributes; // Cần cái này để map cột

namespace FoodDelivery.API.DTOs;

public class ProductImportDto
{
    [Name("TenMon")] // Tên cột trong file CSV
    public string Name { get; set; } = string.Empty;

    [Name("Gia")]
    public decimal Price { get; set; }

    [Name("MoTa")]
    public string? Description { get; set; }

    [Name("MaDanhMuc")] // ID của Category (Ví dụ: 1, 2)
    public int CategoryId { get; set; }

    [Name("LinkAnh")] // Optional: Nếu có link ảnh sẵn thì điền, không thì bỏ trống
    public string? ImageUrl { get; set; }
}