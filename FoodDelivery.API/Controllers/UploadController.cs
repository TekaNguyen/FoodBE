using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodDelivery.API.Services;
using FoodDelivery.API.DTOs;

namespace FoodDelivery.API.Controllers // Thêm dòng này để đưa Class vào đúng Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UploadController(IFileUploadService fileService) : ControllerBase
    {
        [HttpPost("product-image")]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            // Kiểm tra file đầu vào để tránh lỗi Null
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn một file ảnh.");

            var result = await fileService.UploadFileAsync(file, "products");

            if (result == null)
                return BadRequest("File không hợp lệ hoặc không thể lưu trữ.");

            // Trả về URL để Frontend có thể hiển thị ảnh ngay lập tức
            return Ok(new { url = result });
        }

        [HttpPost("chat-image")]
        [AllowAnonymous] // <--- THÊM DÒNG NÀY: Cho phép khách chưa đăng nhập cũng gửi được ảnh chat
        public async Task<IActionResult> UploadChatImage(IFormFile file)
        {
            try
            {
                var imageUrl = await fileService.UploadFileAsync(file, "chats");
                if (string.IsNullOrEmpty(imageUrl)) return BadRequest("Không thể lưu ảnh.");
                return Ok(new { url = imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}