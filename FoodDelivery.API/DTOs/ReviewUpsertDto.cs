using System.ComponentModel.DataAnnotations;

public class ReviewUpsertDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Vui lòng đánh giá từ 1 đến 5 sao")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
    public string Comment { get; set; } = string.Empty;

    public List<string>? Images { get; set; }
}