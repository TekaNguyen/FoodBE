using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Domain.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class BlogCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        // Relationship
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = [];
    }
}