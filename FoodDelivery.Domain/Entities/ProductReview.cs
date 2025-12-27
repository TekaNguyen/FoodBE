using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Domain.Entities;

public class ProductReview : BaseEntity // Khi kế thừa BaseEntity, Id và CreatedAt đã có sẵn
{
    public int ProductId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public string? Reply { get; set; }

    public bool IsApproved { get; set; } = false;

    public List<string> ReviewImages { get; set; } = new();

    // KHÔNG cần khai báo CreatedAt/UpdatedAt ở đây nữa vì BaseEntity đã lo
}