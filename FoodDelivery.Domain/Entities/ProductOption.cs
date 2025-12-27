using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FoodDelivery.Domain.Entities;

public class ProductOption
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // VD: "Size L", "Trân châu trắng"

    public decimal PriceModifier { get; set; } = 0; // Số tiền cộng thêm (VD: 5000)

    // Mối quan hệ: Thuộc về Group nào?
    public int OptionGroupId { get; set; }
    [JsonIgnore]
    public OptionGroup? OptionGroup { get; set; }
}