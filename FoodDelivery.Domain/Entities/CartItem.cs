using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // 👇 TRƯỜNG HỢP 1: User đã đăng nhập (Có thể Null nếu là khách)
        public string? UserId { get; set; }
        // (Nếu bạn có bảng AppUser thì uncomment dòng dưới để giữ quan hệ DB)
        // public virtual AppUser? User { get; set; } 

        // 👇 TRƯỜNG HỢP 2: Khách vãng lai (Lưu mã guest-id tại đây)
        public string? SessionId { get; set; }

        public int Quantity { get; set; }

        public virtual ICollection<CartItemOption> SelectedOptions { get; set; } = [];
    }
}

//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace FoodDelivery.Domain.Entities
//{
//    public class CartItem
//    {
//        [Key]
//        public int Id { get; set; }

//        // Món ăn nào?
//        public int ProductId { get; set; }
//        [ForeignKey("ProductId")]
//        public Product? Product { get; set; } // Link tới bảng Product

//        // Của ai? (User nào)
//        public string UserId { get; set; } = string.Empty;
//        [ForeignKey("UserId")]
//        public AppUser? User { get; set; } // Link tới bảng User

//        // Số lượng bao nhiêu?
//        public int Quantity { get; set; }
//        // 👇👇 BẠN ĐANG THIẾU DÒNG NÀY (Thêm vào ngay nhé)
//        public virtual ICollection<CartItemOption> SelectedOptions { get; set; } = [];
//    }
//}
