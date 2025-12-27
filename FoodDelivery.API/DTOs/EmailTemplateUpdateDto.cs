namespace FoodDelivery.API.DTOs; // Phải khớp với thư mục thực tế

public class EmailTemplateUpdateDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}