using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Domain.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Thumbnail { get; set; } // Đường dẫn ảnh đại diện

        [MaxLength(1000)]
        public string? Summary { get; set; } // Tóm tắt ngắn (hiển thị ở list bài viết)

        // Nội dung chi tiết (Lưu HTML từ Editor như CKEditor/TinyMCE)
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        // --- TRẠNG THÁI & LÊN LỊCH ---
        public bool IsPublished { get; set; } = false;

        // Ngày xuất bản (Cho phép set ngày tương lai để lên lịch post bài)
        public DateTime? PublishedAt { get; set; }

        public int ViewCount { get; set; } = 0;

        // --- SEO FIELDS (Quan trọng cho Marketing) ---
        [MaxLength(255)]
        public string? SeoTitle { get; set; }

        [MaxLength(500)]
        public string? SeoDescription { get; set; }

        [MaxLength(255)]
        public string? SeoKeywords { get; set; }

        // --- RELATIONSHIP (Khóa ngoại) ---
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        // Navigation property để truy xuất ngược lại Category từ bài viết
        public virtual BlogCategory Category { get; set; } = null!;

        // Tracking thời gian
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}