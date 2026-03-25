using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Domain.Entities;

public class NewsletterSubscription : BaseEntity
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? UnsubscribedAt { get; set; }
}