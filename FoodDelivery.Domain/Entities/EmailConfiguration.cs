using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Domain.Entities
{
    public class EmailConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = "SMTP"; // "SMTP", "SendGrid", "Mailgun"

        [Required]
        [MaxLength(200)]
        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        [Required]
        [MaxLength(200)]
        public string FromEmail { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FromName { get; set; } = "FoodDelivery Support";

        [MaxLength(200)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(500)]
        // Lưu ý: Trong thực tế nên mã hóa chuỗi này trước khi lưu xuống DB
        public string Password { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;

        // Đánh dấu cấu hình này đang được sử dụng (nếu hệ thống có nhiều config)
        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}