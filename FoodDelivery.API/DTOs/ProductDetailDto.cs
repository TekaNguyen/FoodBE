namespace FoodDelivery.API.DTOs;

// 1. Khuôn cho từng lựa chọn nhỏ (VD: Size L, Trân châu đen)
public class ProductOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; } // Giá cộng thêm (VD: 5000)
}

// 2. Khuôn cho nhóm lựa chọn (VD: Chọn Size, Topping)
public class OptionGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }    // Bắt buộc chọn?
    public bool AllowMultiple { get; set; } // Chọn nhiều được ko?

    // Chứa danh sách các lựa chọn con
    public List<ProductOptionDto> Options { get; set; } = [];
}

// 3. Khuôn tổng hợp cho Món ăn (Bao gồm cả list nhóm ở trên)
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    // 👇 QUAN TRỌNG NHẤT: Danh sách các nhóm tùy chọn
    public List<OptionGroupDto> OptionGroups { get; set; } = [];
}