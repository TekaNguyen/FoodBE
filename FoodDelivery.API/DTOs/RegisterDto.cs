using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        // Identity mặc định yêu cầu: >6 ký tự, có chữ hoa, thường, số, ký tự đặc biệt
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;
    }
}