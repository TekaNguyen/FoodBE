//namespace FoodDelivery.API.DTOs
//{
//    public class CartItemDto
//    {
//        public int Id { get; set; } // ID của dòng trong giỏ (để xóa)

//        public int ProductId { get; set; } // Để bấm vào quay lại trang chi tiết
//        public string ProductName { get; set; } = string.Empty;
//        public string ProductImage { get; set; } = string.Empty;

//        // 👇 [NÂNG CẤP] Đổi tên thành UnitPrice cho chuẩn
//        // Giá này = Giá gốc + Giá Size + Giá Topping
//        public decimal UnitPrice { get; set; }

//        public int Quantity { get; set; }

//        // Tự động tính tổng: (Giá gốc + Topping) * Số lượng
//        public decimal TotalPrice => UnitPrice * Quantity;

//        // 👇 [MỚI] Hiển thị tên các Option đã chọn cho khách xem
//        // Ví dụ: ["Size L", "Trân châu đen", "Ít đá"]
//        public List<string> SelectedOptions { get; set; } = new();
//    }
//}