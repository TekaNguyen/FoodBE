namespace FoodDelivery.Domain.Entities;

public class EmailSetting
{
    public int Id { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Email { get; set; } = string.Empty; // Email gửi đi
    public string Password { get; set; } = string.Empty; // App Password (không phải pass đăng nhập)
    public string DisplayName { get; set; } = "Food Delivery Support";
    public bool EnableSsl { get; set; } = true;
}