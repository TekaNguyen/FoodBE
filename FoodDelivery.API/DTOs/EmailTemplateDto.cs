namespace FoodDelivery.API.DTOs;

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
}