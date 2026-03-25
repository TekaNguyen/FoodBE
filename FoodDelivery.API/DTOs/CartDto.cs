using System.ComponentModel.DataAnnotations; // Để dùng [Range], [Required]

namespace FoodDelivery.API.DTOs
{
    // ==========================================
    // 1. INPUT: Dữ liệu khách gửi lên để THÊM vào giỏ
    // ==========================================
    public class AddToCartDto
    {
        [Required]
        public int ProductId { get; set; }

        // 👇 Giữ lại dòng validate xịn của bạn
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; } = 1;

        // UserId (tạm thời để string test, sau này dùng Token tự lấy)
        public string UserId { get; set; } = string.Empty;

        // 👇 [QUAN TRỌNG - BẠN ĐANG THIẾU CÁI NÀY]
        // Danh sách ID của Size và Topping (VD: [2, 5] là Size L + Pudding)
        public List<int> OptionIds { get; set; } = [];
    }

    // ==========================================
    // 2. OUTPUT: Dữ liệu trả về để HIỂN THỊ giỏ hàng
    // ==========================================
    public class CartItemDto
    {
        public int Id { get; set; } // ID dòng cart (để xóa)

        public int ProductId { get; set; } // Để bấm vào quay lại trang chi tiết
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;

        // 👇 Giá này = Giá gốc + Giá Size + Giá Topping
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        // Tự động tính tổng tiền: UnitPrice * Số lượng
        public decimal TotalPrice => UnitPrice * Quantity;

        // 👇 List tên Option để hiện lên App (VD: "Size L, Trân châu đen")
        public List<string> SelectedOptions { get; set; } = [];
    }
}