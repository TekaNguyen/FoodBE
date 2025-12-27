namespace FoodDelivery.API.DTOs; // <--- Thêm dòng này ở đầu file

public class PageDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "[]";

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsPublished { get; set; }
}