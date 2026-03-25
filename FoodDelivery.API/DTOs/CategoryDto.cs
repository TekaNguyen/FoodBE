namespace FoodDelivery.API.DTOs; // <--- Phải có .Blog
public class CategoryDto
{
    // Thêm "= string.Empty;" để đảm bảo không bao giờ null
    public string Name { get; set; } = string.Empty;
}