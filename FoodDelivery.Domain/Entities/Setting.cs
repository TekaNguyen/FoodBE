using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Domain.Entities;

public class Setting
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Key { get; set; } = string.Empty; // Ví dụ: "hotline", "shipping_fee"

    [Required]
    public string Value { get; set; } = string.Empty; // Ví dụ: "0909123456", "30000"

    public string? Description { get; set; } // Mô tả để Admin đọc hiểu cái này là gì
}