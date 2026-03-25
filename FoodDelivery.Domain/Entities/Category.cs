using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FoodDelivery.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // Ví dụ: Đồ ăn, Nước uống, Tráng miệng

    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0; // Số càng nhỏ hiện càng cao

    // Relationship: Một danh mục có nhiều món ăn
    // JsonIgnore để tránh bị lặp vô tận khi API trả về
    [JsonIgnore]
    public List<Product> Products { get; set; } = [];
}