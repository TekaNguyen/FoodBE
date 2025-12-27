public class PageUpsertDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "[]"; // Đây là nơi nhận chuỗi JSON từ Frontend
    public bool IsPublished { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}