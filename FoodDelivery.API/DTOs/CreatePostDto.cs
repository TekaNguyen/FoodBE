namespace FoodDelivery.API.DTOs;

public class CreatePostDto
{
    // Gán giá trị mặc định cho các trường bắt buộc
    public string Title { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int CategoryId { get; set; }

    // Các trường này có dấu ? (nullable) rồi thì không cần gán
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
}