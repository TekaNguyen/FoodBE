namespace FoodDelivery.API.DTOs;

public class CreateSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateSettingDto
{
    // Chỉ cho phép sửa Giá trị và Mô tả (Key là cố định, sửa Key là lỗi Code ngay)
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}