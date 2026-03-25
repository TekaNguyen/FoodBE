using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs;

public class UpdateCartDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
    public int Quantity { get; set; }
}