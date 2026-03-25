using System;
using System.ComponentModel.DataAnnotations;

// 👇 SỬA NAMESPACE Ở ĐÂY:
namespace FoodDelivery.Domain.Entities
{
    public class KitchenProductionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}