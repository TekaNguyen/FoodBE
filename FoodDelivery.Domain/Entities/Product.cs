using System.ComponentModel.DataAnnotations;
namespace FoodDelivery.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // 👇 THÊM 2 DÒNG NÀY
    public int? CategoryId { get; set; } // Cho phép null (để tránh lỗi dữ liệu cũ)
    public Category? Category { get; set; } // Navigation Property

    // 👇 THÊM CỘT NÀY
    public int StockQuantity { get; set; } = 0; // Mặc định là 0

    // Trạng thái: True = Đang bán, False = Ngưng kinh doanh
    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0; // Số càng nhỏ hiện càng cao

    public DateTime CreatedAt { get; set; }
    // 👇 ĐẢM BẢO DÒNG NÀY ĐÃ CÓ
    public virtual ICollection<OptionGroup> OptionGroups { get; set; } = [];
}


