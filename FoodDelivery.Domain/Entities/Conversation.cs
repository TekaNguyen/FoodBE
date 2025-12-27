using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        // Index để tìm kiếm session nhanh hơn khi khách truy cập
        public string SessionId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        [MaxLength(150)]
        public string? EmailOrPhone { get; set; }

        [MaxLength(100)]
        // Tên khách hàng (Nickname) nhập lúc bắt đầu chat
        public string? CustomName { get; set; }

        // Mặc định lấy giờ hiện tại khi tạo cuộc hội thoại
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public bool IsClosed { get; set; }

        // Quan trọng: Dùng để sắp xếp hội thoại nào có tin nhắn mới nhất lên đầu danh sách Admin
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        // Navigation Property: Danh sách các tin nhắn trong cuộc hội thoại này
        public virtual ICollection<ChatMessage> Messages { get; set; } = [];

        // --- BỔ SUNG THÊM: Helper Property ---
        [NotMapped] // Không lưu xuống Database, chỉ dùng để hiển thị ở Code
        public string DisplayName => !string.IsNullOrEmpty(CustomName)
            ? CustomName
            : (!string.IsNullOrEmpty(EmailOrPhone) ? EmailOrPhone : "Khách vãng lai");

        // 1. Lưu ID người phụ trách
        public string? AssignedToUserId { get; set; }

        // 2. Lưu tên (để hiển thị nhanh, không cần Join bảng)
        public string? AssignedToUserName { get; set; }

        // 3. [QUAN TRỌNG] Tạo mối quan hệ khóa ngoại an toàn
        // Cái này giúp DB hiểu AssignedToUserId trỏ sang bảng User
        [ForeignKey("AssignedToUserId")]
        public virtual AppUser? AssignedToUser { get; set; }
    }
}