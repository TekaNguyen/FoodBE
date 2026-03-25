using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Cần thiết để dùng attribute [Index]

namespace FoodDelivery.Domain.Entities
{
    // Đánh index cho Slug để query nhanh và đảm bảo không trùng URL
    [Index(nameof(Slug), IsUnique = true)]
    public class Page
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        // URL thân thiện (ví dụ: "ve-chung-toi", "chinh-sach-bao-mat")
        public string Slug { get; set; } = string.Empty;

        // --- CỐT LÕI CỦA PAGE BUILDER ---
        // Lưu cấu trúc JSON của các block (Hero, Text, Image...)
        // Với PostgreSQL: Dùng "jsonb" để có thể query sâu vào JSON nếu cần
        [Column(TypeName = "jsonb")]
        public string ContentJson { get; set; } = "[]";

        // Trạng thái
        public bool IsPublished { get; set; } = false;

        // --- SEO METADATA ---
        [MaxLength(300)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        // --- TRACKING ---
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}