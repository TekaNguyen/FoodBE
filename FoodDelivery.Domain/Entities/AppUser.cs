using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Domain.Entities
{
    // Kế thừa IdentityUser để có sẵn toàn bộ tính năng bảo mật của Microsoft
    public class AppUser : IdentityUser
    {
        // Thêm các trường riêng biệt cho dự án Food Delivery
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; }

    }
}