using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDelivery.Domain.Entities
{
    [Index(nameof(Key), IsUnique = true)] // Đổi tên index
    public class EmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty; // Đổi từ TemplateKey -> Key

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;
        [Column(TypeName = "text")] // PostgreSQL dùng 'text' thay vì 'nvarchar(max)'
        public string Body { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}