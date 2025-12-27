using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        // 👇 QUAN TRỌNG: Dùng tên CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Bổ sung cột này
        public string MessageType { get; set; } = "text"; // "text" hoặc "image"
        public int ConversationId { get; set; }

        [ForeignKey("ConversationId")]
        public Conversation? Conversation { get; set; }
    }
}