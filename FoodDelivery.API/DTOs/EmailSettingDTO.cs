// Đổi từ FoodDelivery.Application.DTOs.Email
namespace FoodDelivery.API.DTOs;

public class EmailSettingDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}