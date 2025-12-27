using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities;

public class Wishlist
{
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public AppUser? User { get; set; } // 👈 SỬA THÀNH AppUser

    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public DateTime AddedAt { get; set; }
}