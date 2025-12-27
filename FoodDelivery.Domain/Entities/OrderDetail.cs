using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá tại thời điểm mua

        // 👇👇 THÊM DÒNG NÀY ĐỂ HẾT LỖI
        public string ProductName { get; set; } = string.Empty;

        // 👇 Dòng này nãy mình đã thêm rồi (lưu Size/Topping)
        public string OptionsSummary { get; set; } = string.Empty;
    }
}